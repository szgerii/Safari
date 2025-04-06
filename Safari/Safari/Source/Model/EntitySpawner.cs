using Engine;
using Microsoft.Xna.Framework;
using Safari.Objects.Entities;
using Safari.Scenes;
using System;

namespace Safari.Model; 

public class EntitySpawner<T> : GameObject where T : notnull, Entity {
	public DateTime LastSpawnAttempt { get; private set; } = DateTime.MinValue;
	public DateTime? LastSuccessfulSpawn { get; private set; } = null;
	public bool Active { get; set; } = true;
	public Rectangle? SpawnArea { get; set; } = null;

	public int Frequency { get; set; }
	public float BaseChance { get; set; } = 1f;
	public float ChanceIncrease { get; set; } = 0f;

	public int EntityLimit { get; set; } = -1;
	public Func<int> EntityCount { get; set; } = () => {
		int count = 0;

		foreach (GameObject obj in GameScene.Active.GameObjects) {
			if (obj is T) {
				count++;
			}
		}

		return count;
	};

	public float CurrentChance { get; private set; }

	public EntitySpawner(int frequency) : base(Vector2.Zero) {
		Frequency = frequency;
	}

	public EntitySpawner(int frequency, float baseChance, float chanceIncrease) : base(Vector2.Zero) {
		Frequency = frequency;
		BaseChance = baseChance;
		ChanceIncrease = chanceIncrease;
	}

	private readonly Random rand = new();
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

		if (CurrentChance == 0) {
			CurrentChance = BaseChance;
		}

		bool randCheck = rand.NextSingle() <= CurrentChance;

		CurrentChance = Math.Min(CurrentChance + ChanceIncrease, 1f);

		if (!randCheck) {
			return;
		}

		Vector2 pos = Utils.GetRandomPosition(SpawnArea ?? GameScene.Active.Model.Level.PlayAreaBounds);
		pos = new(100, 100);
		T entity = (T)Activator.CreateInstance(typeof(T), [ pos ]);
		GameScene.Active.AddObject(entity);
		LastSuccessfulSpawn = LastSpawnAttempt;
		CurrentChance = 0;
	}
}
