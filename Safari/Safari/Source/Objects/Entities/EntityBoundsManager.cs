using Engine.Debug;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Tourists;
using System.Collections.Generic;

namespace Safari.Objects.Entities;

public static class EntityBoundsManager {
	private static QuadTree<Entity> quadTree;

	static EntityBoundsManager() {
		DebugMode.AddFeature(new LoopedDebugFeature("draw-entity-quadtree", (object _, GameTime gameTime) => {
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

	public static void AddEntity(Entity entity) {
		if (entity is Tourist or Jeep) return;

		if (quadTree.Bounds.Contains(entity.Bounds)) {
			quadTree.Insert(entity);
		}
	}

	public static void RemoveEntity(Entity entity) {
		if (entity is Tourist or Jeep) return;

		if (quadTree.Bounds.Contains(entity.Bounds)) {
			quadTree.Remove(entity);
		}
	}

	public static List<Entity> GetEntitiesInArea(Vectangle area) {
		return quadTree.CalculateCollisions(area);
	}
}
