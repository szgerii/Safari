using Engine;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Safari.Model.Entities;
using Safari.Scenes;
using System;

namespace Safari.Model;

/// <summary>
/// Game object for spawning a type of Entity periodically
/// </summary>
/// <typeparam name="T">The type of Entity to spawn</typeparam>
[JsonObject(MemberSerialization.OptIn)]
public class EntitySpawner<T> : GameObject where T : notnull, Entity {
	/// <summary>
	/// The last time the object tried to spawn an entity <br/>
	/// NOTE: it might have been unsuccessful, see <see cref="LastSuccessfulSpawn"/>
	/// </summary>
	[JsonProperty]
	public DateTime LastSpawnAttempt { get; private set; } = DateTime.MinValue;
	/// <summary>
	/// The last time the object successfully spawned an entity
	/// </summary>
	[JsonProperty]
	public DateTime? LastSuccessfulSpawn { get; private set; } = null;
	/// <summary>
	/// Whether to try to spawn entities
	/// </summary>
	[JsonProperty]
	public bool Active { get; set; } = true;
	/// <summary>
	/// The bounds in which the created entity will be placed <br/>
	/// Set to null (default) to use the current level's play area
	/// </summary>
	[JsonProperty]
	public Rectangle? SpawnArea { get; set; } = null;

	/// <summary>
	/// In-game hours between spawn attempts
	/// </summary>
	[JsonProperty]
	public double Frequency { get; set; }
	[JsonProperty]
	private float baseChance = 1f;
	/// <summary>
	/// The starting chance for actually spawning an entity on an attempt
	/// </summary>
	public float BaseChance {
		get => baseChance;
		set {
			baseChance = value;
			CurrentChance = Math.Max(baseChance, CurrentChance);
		}
	}
	/// <summary>
	/// The amount with which <see cref="CurrentChance"/> is increased by after every failed spawn attempt
	/// </summary>
	[JsonProperty]
	public float ChanceIncrease { get; set; } = 0f;
	/// <summary>
	/// The chance that will be used for the next spawn attempt <br/>
	/// Increased by <see cref="ChanceIncrease"/> after every failed attempt, and reset to <see cref="BaseChance"/> after every successful one
	/// </summary>
	[JsonProperty]
	public float CurrentChance { get; private set; }

	/// <summary>
	/// The number of entities that can at most exist in the scene for the spawner to actually attempt to create another one <br/>
	/// Set to -1 for unlimited number of entities
	/// </summary>
	[JsonProperty]
	public int EntityLimit { get; set; } = -1;
	/// <summary>
	/// The method that is used for determining how many entities of type <typeparamref name="T"/> are in the scene <br/>
	/// Modify to something like <code>() => GameScene.Active.Model.XYCount</code> if possible
	/// </summary>
	public Func<int> EntityCount { get; set; } = () => {
		int count = 0;

		foreach (GameObject obj in GameScene.Active.GameObjects) {
			if (obj is T) {
				count++;
			}
		}

		return count;
	};

	/// <summary>
	/// Add any extra spawning conditions here
	/// </summary>
	public Func<bool> ExtraCondition { get; set; } = () => true;

	[JsonConstructor]
	public EntitySpawner() : base(Vector2.Zero) { }

	/// <param name="frequency">The number of in-game hours between attempts</param>
	public EntitySpawner(double frequency) : base(Vector2.Zero) {
		Frequency = frequency;
	}

	/// <param name="frequency">The number of in-game hours between attempts</param>
	/// <param name="baseChance">The base chance for spawning an entity after <see cref="Frequency"/> in-game hours</param>
	/// <param name="chanceIncrease">The chance that base chance is increased by after every failed attempt</param>
	public EntitySpawner(double frequency, float baseChance, float chanceIncrease) : base(Vector2.Zero) {
		Frequency = frequency;
		BaseChance = baseChance;
		ChanceIncrease = chanceIncrease;
		CurrentChance = BaseChance;
	}

	public override void Update(GameTime gameTime) {
		if (!Active) return;

		if ((GameScene.Active.Model.IngameDate - LastSpawnAttempt).TotalHours >= Frequency) {
			TrySpawn();
		}

		base.Update(gameTime);
	}

	private void TrySpawn() {
		LastSpawnAttempt = GameScene.Active.Model.IngameDate;
		
		if (EntityLimit != -1 && EntityCount() >= EntityLimit) {
			return;
		}

		if (!ExtraCondition()) {
			return;
		}

		if (CurrentChance == 0) {
			CurrentChance = BaseChance;
		}

		bool randCheck = Game.Random!.NextSingle() <= CurrentChance;

		CurrentChance = Math.Min(CurrentChance + ChanceIncrease, 1f);

		if (!randCheck) {
			return;
		}

		Vector2 pos = Utils.GetRandomPosition(SpawnArea ?? GameScene.Active.Model.Level!.PlayAreaBounds);
		T entity = (T)Activator.CreateInstance(typeof(T), [ pos ])!;
		Game.AddObject(entity);
		LastSuccessfulSpawn = LastSpawnAttempt;
		CurrentChance = 0;
	}
}
