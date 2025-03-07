using Engine.Scenes;

namespace Safari.Scenes;

public class GameScene : Scene {
	public static GameScene Active => SceneManager.Active as GameScene;

	public override void Unload() {
		// PostUpdate -= CollisionManager.PostUpdate;
		
		base.Unload();
	}

	public override void Load() {
		// CollisionManager.Init(numOfCellsInRow, numOfCellsInCol, cellSize);
		// PostUpdate += CollisionManager.PostUpdate;

		base.Load();
	}
}
