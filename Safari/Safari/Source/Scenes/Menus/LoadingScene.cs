using Engine;
using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Helpers;
using Safari.Model;

namespace Safari.Scenes.Menus;

public class LoadingScene : MenuScene, IResettableSingleton, IUpdatable {
	private static LoadingScene instance;
	private static GameScene gameToLoad = null;
	private static bool loadGame = false;
	public static LoadingScene Instance {
		get {
			instance ??= new();
			return instance;
		}
	}
	public static void ResetSingleton() {
		instance?.Unload();
		instance = null;
	}

	private Label text = null;

	public void LoadNewGame(string parkName, GameDifficulty difficulty) {
		gameToLoad = new GameScene(parkName, difficulty);
		SceneManager.Load(instance);
        loadGame = true;
	}

    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0, 0), PanelSkin.Default, Anchor.TopLeft);
        text = new Label("The game is loading. Please be patient!", Anchor.Center, new Vector2(-1));
        panel.AddChild(text);
    }

    protected override void DestroyUI() {
        panel = null;
        text = null;
    }

    public override void Update(GameTime gameTime) {
        if (loadGame) {
            SceneManager.Load(gameToLoad);
            loadGame = false;
			gameToLoad = null;
        }
    }
}
