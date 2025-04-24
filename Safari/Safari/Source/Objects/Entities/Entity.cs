using Engine;
using Engine.Components;
using Engine.Debug;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using Safari.Model;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Objects.Entities;

[SimulationActor]
public abstract class Entity : GameObject, ISpatial {
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
	/// <summary>
	/// Invoked exactly once at the end of an entity's lifetime
	/// </summary>
	public event EventHandler Died;

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
	/// The calculated bounds for the current frame, invalidated (set to null) on PreUpdate
	/// </summary>
	private Vectangle calcBounds = new(-1, -1, -1, -1);
	private Vectangle? boundsBaseOverride;
	/// <summary>
	/// The bounding rectangle of the entity's display content <br/>
	/// The setter can be used to override its BASE rectangle, which is then scaled up by the Sprite scaling.
	/// To later turn off the override, set this to null.
	/// </summary>
	public Vectangle Bounds {
		get => calcBounds;
		set {
			boundsBaseOverride = value;
		}
	}

	/// <summary>
	/// The absolute position of the entity's center point
	/// </summary>
	public Vector2 CenterPosition => Bounds.Center;

	/// <summary>
	/// The number of tiles the entity can see in any direction
	/// </summary>
	public float SightDistance { get; set; } = 5;
	/// <summary>
	/// The number of tiles the entity can interact with in any direction
	/// </summary>
	public float ReachDistance { get; set; } = 1;
	
	/// <summary>
	/// Bool for controlling whether this entity is visible (without any nearby light) at night
	/// </summary>
	public bool VisibleAtNight { get; set; } = true;
	/// <summary>
	/// Getter for checking whether this entity is currently visible to the player
	/// </summary>
	public bool Visible {
		get {
			GameModel model = GameScene.Active.Model;
			Level level = model.Level;
			int tileSize = GameScene.Active.Model.Level.TileSize;
			Vector2 offset = Bounds.Size / 2;
			Vector2 centerPoint = Position + offset;
			int map_x = (int)(centerPoint.X / level.TileSize);
			int map_y = (int)(centerPoint.Y / level.TileSize);
			if (!model.IsDaytime && !VisibleAtNight && !model.Level.LightManager.CheckLight(map_x, map_y)) {
				return false;
			}
			return true;
		}
	}

	/// <summary>
	/// The game world bounding box of the animal's vision
	/// </summary>
	public Vectangle SightArea {
		get {
			int tileSize = GameScene.Active.Model.Level.TileSize;
			Vector2 centerOffset = Bounds.Size / 2;
			Vector2 startPoint = Position + centerOffset - new Vector2(SightDistance * tileSize);
			float size = 2 * SightDistance * tileSize;

			return new(startPoint.X, startPoint.Y, size, size);
		}
	}
	/// <summary>
	/// The game world bounding box of the animal's reach
	/// </summary>
	public Vectangle ReachArea {
		get {
			int tileSize = GameScene.Active.Model.Level.TileSize;
			Vector2 centerOffset = Bounds.Size / 2;
			Vector2 startPoint = Position + centerOffset - new Vector2(ReachDistance * tileSize);
			float size = 2 * ReachDistance * tileSize;

			return new(startPoint.X, startPoint.Y, size, size);
		}
	}

	/// <summary>
	/// Indicates if the entity has died
	/// </summary>
	public bool IsDead { get; private set; } = false;

	/// <summary>
	/// The navigation component controlling the entity's movement
	/// </summary>
	public NavigationCmp NavCmp { get; protected set; }

	/// <summary>
	/// The sprite component of the entity used for rendering
	/// </summary>
	public SpriteCmp Sprite { get; protected set; }

	/// <summary>
	/// Whether the entity is currently being inspected through the entity controller
	/// </summary>
	public bool IsBeingInspected { get; set; }

	private DateTime lastHourUpdate;
	private DateTime lastDayUpdate;
	private DateTime lastWeekUpdate;

	public Entity(Vector2 pos) : base(pos) {
		NavCmp = new NavigationCmp();
		Attach(NavCmp);
	}

	static Entity() {
		DebugMode.AddFeature(new LoopedDebugFeature("entity-interact-bounds", (object sender, GameTime gameTime) => {
			foreach (Entity e in ActiveEntities) {
				e.DrawInteractBounds(gameTime);
			}
		}, GameLoopStage.POST_DRAW));
	}

	/// <summary>
	/// Retrieves the active and alive entities in a given area
	/// </summary>
	/// <param name="area">The area to filter for</param>
	/// <returns>The list of entities inside the area</returns>
	public static List<Entity> GetEntitiesInArea(Vectangle area) {
		return EntityBoundsManager.GetEntitiesInArea(area);
	}

	/// <summary>
	/// Updates the entity's Bounds property
	/// </summary>
	public void UpdateBounds() {
		Vectangle result;

		if (boundsBaseOverride != null) {
			Vectangle @base = boundsBaseOverride.Value;
			result = new Vectangle(Position - (Sprite?.Origin ?? Vector2.Zero) + @base.Location, @base.Size);
		} else if (Sprite is AnimatedSpriteCmp animSprite) {
			result = new Vectangle(Position - animSprite.Origin, new Vector2(animSprite.FrameWidth, animSprite.FrameHeight));
		} else {
			Vector2 size = Sprite.SourceRectangle?.Size.ToVector2() ?? Sprite.Texture.Bounds.Size.ToVector2();
			result = new Vectangle(Position - Sprite.Origin, size);
		}
		result.Size *= Sprite.Scale;

		calcBounds = result;
	}

	public override void Load() {
		GameModel model = GameScene.Active.Model;
		lastHourUpdate = model.IngameDate;
		lastDayUpdate = model.IngameDate;
		lastWeekUpdate = model.IngameDate;

		model.EntityCount++;
		activeEntities.Add(this);
		UpdateBounds();
		EntityBoundsManager.AddEntity(this);

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.EntityCount--;
		activeEntities.Remove(this);
		EntityBoundsManager.RemoveEntity(this);

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

	public override void Draw(GameTime gameTime) {
		if (!Visible) {
			return;
		}

		if (IsBeingInspected) {
			float fadeFactor = (float)Math.Sin(2 * gameTime.TotalGameTime.TotalSeconds) / 4f + 0.25f;
			Sprite.Tint = Color.White * (0.5f + fadeFactor);
		} else {
			Sprite.Tint = Color.White;
		}

		base.Draw(gameTime);
	}

	private Texture2D sightAreaTex = null, reachAreaTex = null, boundsTex = null;
	/// <summary>
	/// Draws the bounds of the entity's vision and reach to the screen (debug feature)
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	public void DrawInteractBounds(GameTime gameTime) {
		if (SightArea.Width != 0 && SightArea.Height != 0) {
			if (sightAreaTex == null || sightAreaTex.Bounds.Size != SightArea.Size.ToPoint()) {
				sightAreaTex = Utils.GenerateTexture((int)SightArea.Width, (int)SightArea.Height, Color.White, true);
			}
			Game.SpriteBatch.Draw(sightAreaTex, (Rectangle)SightArea, null, Color.Orange, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		}

		if (ReachArea.Width != 0 && ReachArea.Height != 0) {
			if (reachAreaTex == null || reachAreaTex.Bounds.Size != ReachArea.Size.ToPoint()) {
				reachAreaTex = Utils.GenerateTexture((int)ReachArea.Width, (int)ReachArea.Height, Color.White, true);
			}
			Game.SpriteBatch.Draw(reachAreaTex, (Rectangle)ReachArea, null, Color.DarkRed, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		}

		if (boundsTex?.Bounds.Size != Bounds.Size.ToPoint()) {
			boundsTex = null;
		}

		if (boundsTex == null && Bounds.Width > 0 && Bounds.Height > 0) {
			boundsTex = Utils.GenerateTexture((int)Bounds.Width, (int)Bounds.Height, new Color(Color.Cyan, 0.2f), false);
		}

		if (boundsTex != null) {
			Game.SpriteBatch.Draw(boundsTex, (Rectangle)Bounds, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, Sprite.RealLayerDepth + 0.01f);
		}
	}

	/// <summary>
	/// Removes the entity from the game
	/// </summary>
	public void Die() {
		if (IsDead) return;

		Died?.Invoke(this, EventArgs.Empty);
		IsDead = true;

		Game.RemoveObject(this);
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
	public bool CanSee(Entity e) => SightArea.Intersects(e.Bounds);
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
	public bool CanReach(Entity e) => ReachArea.Intersects(e.Bounds);

	/// <summary>
	/// Retrieves a list of all other active and alive entities inside the entity's sight
	/// (does not include itself)
	/// </summary>
	/// <returns>The list of active entities inside the entity's vision</returns>
	public List<Entity> GetEntitiesInSight() => GetEntitiesInArea(SightArea);

	/// <summary>
	/// Retrieves a list of all other active and alive entities inside the entity's reach
	/// (does not include itself)
	/// </summary>
	/// <returns>The list of active entities inside the entity's reach</returns>
	public List<Entity> GetEntitiesInReach() => GetEntitiesInArea(ReachArea);

	/// <summary>
	/// Retrieves a list of the non-empty tiles inside the entity's sight
	/// </summary>
	/// <returns>A list of the tiles inside the entity's vision</returns>
	public List<Tile> GetTilesInSight() => GameScene.Active.Model.Level.GetTilesInWorldArea((Rectangle)SightArea);
	/// <summary>
	/// Retrieves a list of the non-empty tiles inside the entity's reach
	/// </summary>
	/// <returns>A list of the tiles that the entity can interact with</returns>
	public List<Tile> GetTilesInReach() => GameScene.Active.Model.Level.GetTilesInWorldArea((Rectangle)ReachArea);

	public override string ToString() {
		return DisplayName;
	}
}
