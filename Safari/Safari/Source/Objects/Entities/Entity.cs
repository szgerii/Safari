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
	public event EventHandler HourPassed;
	/// <summary>
	/// Invoked every time an in-game day passes for this entity
	/// </summary>
	public event EventHandler DayPassed;
	/// <summary>
	/// Invoked every time an in-game week passes for this entity
	/// </summary>
	public event EventHandler WeekPassed;

	private static readonly List<Entity> activeEntities = [];
	/// <summary>
	/// A list of entities that are currently part of the game world
	/// </summary>
	public static IReadOnlyList<Entity> ActiveEntities => activeEntities;

	/// <summary>
	/// Entity name to display in the shop / manager screens
	/// </summary>
	public string DisplayName { get; protected init; }

	/// <summary>
	/// The bounding rectangle of the entity's display content
	/// </summary>
	public Rectangle Bounds {
		get {
			Rectangle result;

			if (sprite is AnimatedSpriteCmp animSprite) {
				result = new Rectangle(Position.ToPoint(), new Point(animSprite.FrameWidth, animSprite.FrameHeight));
			} else {
				result = new Rectangle(Position.ToPoint(), sprite.SourceRectangle?.Size ?? sprite.Texture.Bounds.Size);
			}
			result.Size = (result.Size.ToVector2() * sprite.Scale).ToPoint();

			return result;
		}
	}

	/// <summary>
	/// The number of tiles the entity can see in any direction
	/// </summary>
	public int SightDistance { get; set; } = 4;
	/// <summary>
	/// The number of tiles the entity can interact with in any direction
	/// </summary>
	public int ReachDistance { get; set; } = 1;
	/// <summary>
	/// The game world bounding box of the animal's vision
	/// </summary>
	public Rectangle SightArea {
		get {
			int tileSize = GameScene.Active.Model.Level.TileSize;
			Point centerOffset = Bounds.Size / new Point(2);
			Point startPoint = Position.ToPoint() + centerOffset - new Point(SightDistance * tileSize);

			return new(startPoint, new Point(2 * SightDistance * tileSize));
		}
	}
	/// <summary>
	/// The game world bounding box of the animal's reach
	/// </summary>
	public Rectangle ReachArea {
		get {
			int tileSize = GameScene.Active.Model.Level.TileSize;
			Point centerOffset = Bounds.Size / new Point(2);
			Point startPoint = Position.ToPoint() + centerOffset - new Point(ReachDistance * tileSize);

			return new(startPoint, new Point(2 * ReachDistance * tileSize));
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

	/// <summary>
	/// Retrieves the active entities in a given area
	/// </summary>
	/// <param name="area">The area to filter for</param>
	/// <returns>The list of entities inside the area</returns>
	public static List<Entity> GetEntitiesInArea(Rectangle area) {
		// OPTIMIZE: chunks

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
	/// <summary>
	/// Draws the bounds of the entity's vision and reach to the screen (debug feature)
	/// </summary>
	/// <param name="gameTime">The current game time</param>
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

	/// <summary>
	/// Checks if the entity can see a given position
	/// </summary>
	/// <param name="pos">The position to check</param>
	/// <returns>Whether the position is inside the entity's sight</returns>
	public bool CanSee(Vector2 pos) => SightArea.Contains(pos);
	/// <summary>
	/// Checks if the entity can see a given game object
	/// </summary>
	/// <param name="obj">The game object to check</param>
	/// <returns>Whether the object is inside the entity's sight</returns>
	public bool CanSee(GameObject obj) => CanSee(obj.Position);
	/// <summary>
	/// Checks if the entity can reach a given position
	/// </summary>
	/// <param name="pos">The position to check</param>
	/// <returns>Whether the position is inside the entity's reach</returns>
	public bool CanReach(Vector2 pos) => ReachArea.Contains(pos);
	/// <summary>
	/// Checks if the entity can reach a given game object
	/// </summary>
	/// <param name="obj">The game object to check</param>
	/// <returns>Whether the object is inside the entity's reach</returns>
	public bool CanReach(GameObject obj) => CanReach(obj.Position);

	/// <summary>
	/// Retrieves a list of all other active entities inside the entity's sight
	/// (does not include itself)
	/// </summary>
	/// <returns>The list of active entities inside the entity's vision</returns>
	public List<Entity> GetEntitiesInSight() {
		List<Entity> result = GetEntitiesInArea(SightArea);
		result.Remove(this);
		return result;
	}

	/// <summary>
	/// Retrieves a list of all other active entities inside the entity's reach
	/// (does not include itself)
	/// </summary>
	/// <returns>The list of active entities inside the entity's reach</returns>
	public List<Entity> GetEntitiesInReach() {
		List<Entity> result = GetEntitiesInArea(ReachArea);
		result.Remove(this);
		return result;
	}

	/// <summary>
	/// Retrieves a list of the non-empty tiles inside the entity's sight
	/// </summary>
	/// <returns>A list of the tiles inside the entity's vision</returns>
	public List<Tile> GetTilesInSight() => GameScene.Active.Model.Level.GetTilesInWorldArea(SightArea);
	/// <summary>
	/// Retrieves a list of the non-empty tiles inside the entity's reach
	/// </summary>
	/// <returns>A list of the tiles that the entity can interact with</returns>
	public List<Tile> GetTilesInReach() => GameScene.Active.Model.Level.GetTilesInWorldArea(ReachArea);
}
