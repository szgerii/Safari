using Engine;
using Engine.Components;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Objects.Entities;

[SimulationActor]
public abstract class Entity : GameObject {
	/// <summary>
	/// Invoked every time an in-game hour passes for this entity
	/// </summary>
	public event EventHandler? HourPassed;
	/// <summary>
	/// Invoked every time an in-game day passes for this entity
	/// </summary>
	public event EventHandler? DayPassed;
	/// <summary>
	/// Invoked every time an in-game week passes for this entity
	/// </summary>
	public event EventHandler? WeekPassed;

	private static readonly List<Entity> activeEntities = [];
	public static IReadOnlyList<Entity> ActiveEntities => activeEntities;

	/// <summary>
	/// Entity name to display in the shop / manager screens
	/// </summary>
	public string DisplayName { get; protected init; }

	public int SightDistance { get; set; } = 4;
	public int ReachDistance { get; set; } = 1;
	public Rectangle SightArea {
		get {
			int tileSize = GameScene.Active.Model.Level.TileSize;
			Point offset = (sprite.SourceRectangle?.Size ?? sprite.Texture.Bounds.Size) / new Point(2);
			Point centerPoint = Position.ToPoint() + offset;

			return new(centerPoint - new Point(SightDistance * tileSize), new Point(2 * SightDistance * tileSize));
		}
	}
	public Rectangle ReachArea {
		get {
			int tileSize = GameScene.Active.Model.Level.TileSize;
			Vector2 centerOffset = (sprite.SourceRectangle?.Size ?? sprite.Texture.Bounds.Size).ToVector2() / 2f;
			Vector2 startPoint = Position + centerOffset - new Vector2(ReachDistance * tileSize);

			return new(startPoint.ToPoint(), new Point(2 * ReachDistance * tileSize));
		}
	}

	protected SpriteCmp sprite;

	private DateTime lastHourUpdate;
	private DateTime lastDayUpdate;
	private DateTime lastWeekUpdate;

	public Entity(Vector2 pos) : base(pos) { }

	static Entity() {
		DebugMode.AddFeature(new LoopedDebugFeature("entity-interact-bounds", (object sender, GameTime gameTime) => {
			foreach (Entity e in ActiveEntities) {
				e.DrawInteractBounds(gameTime);
			}
		}, GameLoopStage.POST_DRAW));
	}

	public static List<Entity> GetEntitiesInArea(Rectangle area) {
		List<Entity> results = [];

		foreach (Entity e in ActiveEntities) {
			if (area.Contains(e.Position)) {
				results.Add(e);
			}
		}

		return results;
	}

	public override void Load() {
		GameModel model = GameScene.Active.Model;
		lastHourUpdate = model.IngameDate;
		lastDayUpdate = model.IngameDate;
		lastWeekUpdate = model.IngameDate;

		model.EntityCount++;
		activeEntities.Add(this);

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.EntityCount--;
		activeEntities.Remove(this);

		base.Unload();
	}

	public override void Update(GameTime gameTime) {
		DateTime ingameDate = GameScene.Active.Model.IngameDate;
		if (ingameDate - lastHourUpdate > TimeSpan.FromHours(1)) {
			HourPassed?.Invoke(this, EventArgs.Empty);
			lastHourUpdate = ingameDate;
		}
		if (ingameDate - lastDayUpdate > TimeSpan.FromDays(1)) {
			DayPassed?.Invoke(this, EventArgs.Empty);
			lastDayUpdate = ingameDate;
		}
		if (ingameDate - lastWeekUpdate > TimeSpan.FromDays(7)) {
			WeekPassed?.Invoke(this, EventArgs.Empty);
			lastWeekUpdate = ingameDate;
		}
		base.Update(gameTime);
	}

	private Texture2D sightAreaTex = null, reachAreaTex = null;
	public void DrawInteractBounds(GameTime gameTime) {
		if (sightAreaTex == null || sightAreaTex.Bounds.Size != SightArea.Size) {
			sightAreaTex = Utils.GenerateTexture(SightArea.Width, SightArea.Height, Color.White, true);
		}

		if (reachAreaTex == null || reachAreaTex.Bounds.Size != ReachArea.Size) {
			reachAreaTex = Utils.GenerateTexture(ReachArea.Width, ReachArea.Height, Color.White, true);
		}

		Game.SpriteBatch.Draw(sightAreaTex, SightArea, null, Color.Orange, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		Game.SpriteBatch.Draw(reachAreaTex, ReachArea, null, Color.DarkRed, 0f, Vector2.Zero, SpriteEffects.None, 0f);
	}

	public bool CanSee(Vector2 pos) => SightArea.Contains(pos);
	public bool CanSee(GameObject obj) => CanSee(obj.Position);
	public bool CanReach(Vector2 pos) => ReachArea.Contains(pos);
	public bool CanReach(GameObject obj) => CanReach(obj.Position);

	public List<Entity> GetEntitiesInSight() {
		List<Entity> result = GetEntitiesInArea(SightArea);
		result.Remove(this);
		return result;
	}

	public List<Entity> GetEntitiesInReach() {
		List<Entity> result = GetEntitiesInArea(ReachArea);
		result.Remove(this);
		return result;
	}

	public List<Tile> GetTilesInSight() => GameScene.Active.Model.Level.GetTilesInWorldArea(SightArea);
	public List<Tile> GetTilesInReach() => GameScene.Active.Model.Level.GetTilesInWorldArea(ReachArea);
}
