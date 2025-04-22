using Engine.Debug;
using Engine.Helpers;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Tourists;
using System.Collections.Generic;

namespace Safari.Objects.Entities;

public static class EntityManager {
	private static QuadTree<Entity> quadTree;

	static EntityManager() {
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

		InvalidateCache();

		SceneManager.LoadedScene += OnSceneLoaded;
	}

	public static void CleanUp() {
		InvalidateCache();
		quadTree = null;
		SceneManager.LoadedScene -= OnSceneLoaded;
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

	private readonly static Dictionary<Rectangle, List<Entity>> cache = [];
	public static List<Entity> GetEntitiesInArea(Vectangle area) {
		/*if (cache.TryGetValue(area, out List<Entity> value)) {
			return value;
		}*/

		return quadTree.CalculateCollisions(area);
		//List<Entity> result = quadTree.CalculateCollisions(area);

		//cache[area] = result;
		//return result;
	}

	public static void InvalidateCache() {
		cache.Clear();
	}

	private static bool preUpdateHandlerActive = false;
	private static void OnSceneLoaded(object sender, Scene scn) {
		/*static void preUpdateHandler(object sender, GameTime gametime) {
			InvalidateCache();
		}

		if (scn is GameScene) {
			if (!preUpdateHandlerActive) {
				scn.PreUpdate += preUpdateHandler;
				preUpdateHandlerActive = true;
			}
		} else {
			scn.PreUpdate -= preUpdateHandler;
			preUpdateHandlerActive = false;
		}*/
	}
}
