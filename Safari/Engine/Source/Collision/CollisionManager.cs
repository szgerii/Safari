using Engine.Components;
using Engine.Debug;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.Collision;

public static class CollisionManager {
	private static QuadTree<CollisionCmp> quadTree;
	private static List<CollisionCmp> collisionCmps;

	static CollisionManager() {
		DebugMode.AddFeature(new LoopedDebugFeature("draw-coll-quadtree", DrawQuadTreeNodes, GameLoopStage.POST_DRAW));
		DebugMode.AddFeature(new LoopedDebugFeature("draw-colliders", DrawColliders, GameLoopStage.POST_DRAW));
	}

	public static void Init(Vectangle baseBounds) {
		quadTree = new QuadTree<CollisionCmp>(baseBounds, 5, 10);
		collisionCmps = [];
	}

	public static void CleanUp() {
		quadTree = null;
		collisionCmps = null;
	}

	public static void Insert(CollisionCmp cmp) {
		if (IsOutOfBounds(cmp)) {
			return;
		}

		quadTree.Insert(cmp);
		collisionCmps.Add(cmp);
	}

	public static void Remove(CollisionCmp cmp) {
		quadTree.Remove(cmp);
		collisionCmps.Remove(cmp);
	}

	public static bool Collides(CollisionCmp cmp) => quadTree.Collides(cmp, Targets);

	public static List<CollisionCmp> GetCollisions(CollisionCmp cmp) {
		if (IsOutOfBounds(cmp)) {
			return [];
		}

		return quadTree.CalculateCollisions(cmp, Targets);
	}

	public static bool IsOutOfBounds(CollisionCmp c) {
		return !quadTree.Bounds.Contains(c.Bounds);
	}

	public static bool IsOutOfBounds(Vectangle c) {
		return !quadTree.Bounds.Contains(c);
	}

	// OPTIMIZE: maybe we'll need to optimize how collision differences are stored and calculated
	private static readonly List<(CollisionCmp listener, CollisionCmp other)> activeCollisions = [];
	public static void PostUpdate(object _, GameTime gameTime) {
		List<(CollisionCmp listener, CollisionCmp other)> buff = [];

		foreach (CollisionCmp listener in collisionCmps) {
			if (!listener.HasActiveCollisionEvents) {
				continue;
			}

			List<CollisionCmp> collisions = quadTree.CalculateCollisions(listener, Targets);
			foreach (CollisionCmp other in collisions) {
				buff.Add((listener, other));

				if (activeCollisions.Contains((listener, other))) {
					listener.OnCollisionStay(other);
				} else {
					listener.OnCollisionEnter(other);
				}

				activeCollisions.Add((listener, other));
			}
		}

		for (int i = activeCollisions.Count - 1; i >= 0; i--) {
			if (!buff.Contains(activeCollisions[i])) {
				activeCollisions[i].listener.OnCollisionLeave(activeCollisions[i].other);
				activeCollisions.Remove(activeCollisions[i]);
			}
		}
	}

	private static bool Targets(CollisionCmp a, CollisionCmp b) {
		return (a.Targets & b.Tags) != 0;
	}

	// DEBUG MODE

	public static void DrawQuadTreeNodes(object _, GameTime gameTime) {
		if (quadTree == null) return;

		quadTree.Traverse((QuadTree<CollisionCmp> node) => {
			node.PostDraw(gameTime);
		});
	}

	public static void DrawColliders(object _, GameTime gameTime) {
		if (collisionCmps == null) return;

		foreach (CollisionCmp cmp in collisionCmps) {
			Game.SpriteBatch.Draw(cmp.ColliderTex, cmp.AbsoluteCollider.Location, cmp.ColliderTex.Bounds, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
		}
	}
}
