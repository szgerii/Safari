using Engine.Debug;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Safari.Model.Entities.Tourists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Safari.Model.Entities;

public static class EntityBoundsManager {
	private static QuadTree<Entity>? quadTree;

	static EntityBoundsManager() {
		DebugMode.AddFeature(new LoopedDebugFeature("draw-entity-quadtree", (object? _, GameTime gameTime) => {
			if (quadTree == null) return;

			quadTree.Traverse((QuadTree<Entity> node) => {
				node.PostDraw(gameTime);
			});
		}, GameLoopStage.POST_DRAW));
	}

	public static void Init(Vectangle mapBounds) {
		quadTree = new QuadTree<Entity>(mapBounds, 4, 12) {
			ExpectedCollisionCount = 150
		};
	}

	public static void CleanUp() {
		quadTree = null;
	}

	[MemberNotNull(nameof(quadTree))]
	private static void AssertInit() {
		if (quadTree == null) {
			throw new InvalidOperationException("Cannot access this feature of the EntityBoundsManager before it has been initialized");
		}
	}

	public static void AddEntity(Entity entity) {
		AssertInit();
		if (entity is Tourist or Jeep) return;

		if (quadTree.Bounds.Contains(entity.Bounds)) {
			quadTree.Insert(entity);
		}
	}

	public static void RemoveEntity(Entity entity) {
		AssertInit();
		if (entity is Tourist or Jeep) return;

		if (quadTree.Bounds.Contains(entity.Bounds)) {
			quadTree.Remove(entity);
		}
	}

	public static List<Entity> GetEntitiesInArea(Vectangle area) {
		AssertInit();
		return quadTree.CalculateCollisions(area);
	}
}
