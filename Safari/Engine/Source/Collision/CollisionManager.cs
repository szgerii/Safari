using Engine.Components;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Engine.Collision;

public static class CollisionManager {
	public static int GridWidth { get; private set; }
	public static int GridHeight { get; private set; }
	public static int CellSize { get; private set; }
	public static int MaxX => GridWidth * CellSize;
	public static int MaxY => GridHeight * CellSize;

	private static List<CollisionCmp>[,] collisionMap;
	private static List<CollisionCmp> collisionCmps;

	// debug
	private readonly static Texture2D gridCellTex = Utils.GenerateTexture(1, 1, new Color(Color.Blue, 0.3f));

	static CollisionManager() {
		DebugMode.AddFeature(new LoopedDebugFeature("coll-check-areas", DrawColliderCheckAreas, GameLoopStage.POST_DRAW));
		DebugMode.AddFeature(new LoopedDebugFeature("coll-draw", DrawColliders, GameLoopStage.POST_DRAW));
	}

	public static void Init(int gridWidth, int gridHeight, int cellSize) {
		GridWidth = gridWidth;
		GridHeight = gridHeight;
		CellSize = cellSize;
		
		collisionMap = new List<CollisionCmp>[gridWidth, gridHeight];
		collisionCmps = new List<CollisionCmp>();

		for (int x = 0; x < gridWidth; x++) {
			for (int y = 0; y < gridHeight; y++) {
				collisionMap[x, y] = new();
			}
		}
	}

	public static void Insert(CollisionCmp cmp) {
		Point min = GetGridPos(cmp);
		Point max = GetGridPos(cmp.AbsoluteCollider.BottomRight);

		ClampToCollMap(ref min, ref max);

		for (int x = min.X; x <= max.X; x++) {
			for (int y = min.Y; y <= max.Y; y++) {
				collisionMap[x, y].Add(cmp);
			}
		}
		collisionCmps.Add(cmp);
	}

	public static void Remove(CollisionCmp cmp) {
		Point min = GetGridPos(cmp);
		Point max = GetGridPos(cmp.AbsoluteCollider.BottomRight);

		ClampToCollMap(ref min, ref max);

		for (int x = min.X; x <= max.X; x++) {
			for (int y = min.Y; y <= max.Y; y++) {
				collisionMap[x, y].Remove(cmp);
			}
		}
		collisionCmps.Remove(cmp);
	}

	public static bool Collides(Collider coll, CollisionCmp self = null) {
		Point min = GetGridPos(coll);
		Point max = GetGridPos(coll.BottomRight);

		ClampToCollMap(ref min, ref max);

		for (int x = min.X; x <= max.X; x++) {
			for (int y = min.Y; y <= max.Y; y++) {
				foreach (CollisionCmp cmp in collisionMap[x, y]) {
					bool targets = self == null || Targets(self, cmp);

					if (cmp != self && cmp.Intersects(coll) && targets) {
						return true;
					}
				}
			}
		}

		return false;
	}

	public static List<CollisionCmp> GetCollisions(Collider coll, CollisionCmp self = null) {
		Point min = GetGridPos(coll);
		Point max = GetGridPos(coll.BottomRight);

		ClampToCollMap(ref min, ref max);

		List<CollisionCmp> collisions = new();
		for (int x = min.X; x <= max.X; x++) {
			for (int y = min.Y; y <= max.Y; y++) {
				foreach (CollisionCmp cmp in collisionMap[x, y]) {
					bool targets = self == null || Targets(self, cmp);

					if (cmp != self && !collisions.Contains(cmp) && targets && cmp.Intersects(coll)) {
						collisions.Add(cmp);
					}
				}
			}
		}

		return collisions;
	}

	public static bool IsOutOfBounds(Collider c) {
		return c.X < 0 || c.Y < 0 || c.BottomRight.X >= MaxX || c.BottomRight.Y >= MaxY;
	}

	// OPTIMIZE: maybe we'll need to optimize how collision differences are stored and calculated
	private static List<(CollisionCmp listener, CollisionCmp other)> activeCollisions = new();
	public static void PostUpdate(object _, GameTime gameTime) {
		List<(CollisionCmp listener, CollisionCmp other)> buff = new();

		foreach (CollisionCmp listener in collisionCmps) {
			if (!listener.HasActiveCollisionEvents) {
				continue;
			}

			foreach (CollisionCmp other in GetCollisions(listener.AbsoluteCollider, listener)) {
				if (activeCollisions.Contains((listener, other))) {
					listener.OnCollisionStay(other);
				} else {
					listener.OnCollisionEnter(other);
				}

				buff.Add((listener, other));
			}
		}

		foreach (var collision in activeCollisions) {
			if (!buff.Contains(collision)) {
				collision.listener.OnCollisionLeave(collision.other);
			}
		}

		activeCollisions = buff;
	}

	private static bool Targets(CollisionCmp a, CollisionCmp b) {
		return (a.Targets & b.Tags) != 0;
	}

	private static Point GetGridPos(Vector2 pos) {
		int x = (int)(pos.X / CellSize);
		int y = (int)(pos.Y / CellSize);

		return new Point(x, y);
	}
	private static Point GetGridPos(Collider coll) => GetGridPos(coll.Position);
	private static Point GetGridPos(CollisionCmp cmp) => GetGridPos(cmp.AbsoluteCollider.Position);

	/// <summary>
	/// Clamps a min-max Point pair to the collision map's bounds
	/// </summary>
	private static void ClampToCollMap(ref Point min, ref Point max) {
		min.X = Math.Max(min.X, 0);
		min.Y = Math.Max(min.Y, 0);
		max.X = Math.Min(max.X, collisionMap.GetLength(0));
		max.Y = Math.Min(max.Y, collisionMap.GetLength(1));
	}

	// DEBUG MODE

	public static void DrawColliderCheckAreas(object _, GameTime gameTime) {
		if (collisionMap == null) return;
		
		for (int x = 0; x < GridWidth; x++) {
			for (int y = 0; y < GridHeight; y++) {
				if (collisionMap[x, y].Count != 0) {
					Rectangle rect = new Rectangle(x * CellSize, y * CellSize, CellSize, CellSize);
					Game.SpriteBatch.Draw(gridCellTex, rect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.05f);
				}
			}
		}
	}

	public static void DrawColliders(object _, GameTime gameTime) {
		if (collisionCmps == null) return;

		foreach (CollisionCmp cmp in collisionCmps) {
			Game.SpriteBatch.Draw(cmp.ColliderTex, cmp.AbsoluteCollider.Position, cmp.ColliderTex.Bounds, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
		}
	}
}
