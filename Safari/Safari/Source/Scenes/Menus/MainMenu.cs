using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Engine;
using Safari.Helpers;
using Engine.Input;
using Microsoft.Xna.Framework.Input;
using Safari.Source.Persistence;

namespace Safari.Scenes.Menus;
public class MainMenu : MenuScene, IUpdatable, IResettableSingleton {
	private static MainMenu instance;
	public static MainMenu Instance {
		get {
			instance ??= new();
			return instance;
		}
	}
	public static void ResetSingleton() {
        instance?.Unload();
		instance = null;
	}

	private Header title;
    private Panel buttonPanel;
    private Button newGameButton;
    private Button continueGameButton;
    private Button loadGameButton;
    private Button settingsButton;
    private Button exitButton;
    private bool loadGame = false;

    protected override void ConstructUI() {
        this.panel = new Panel(new Vector2(0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);

        buttonPanel = new Panel(new Vector2(0.3f, 0.75f), PanelSkin.None, Anchor.Center);

        newGameButton = new Button("New Game", ButtonSkin.Default, Anchor.AutoCenter);
        newGameButton.OnClick = NewGameClicked;
        buttonPanel.AddChild(newGameButton);

        continueGameButton = new Button("Continue Game", ButtonSkin.Default, Anchor.AutoCenter);
        continueGameButton.OnClick = ContinueGameClicked;
        buttonPanel.AddChild(continueGameButton);

        loadGameButton = new Button("Load Game", ButtonSkin.Default, Anchor.AutoCenter);
        loadGameButton.OnClick = LoadGameClicked;
        buttonPanel.AddChild(loadGameButton);

        settingsButton = new Button("Settings", ButtonSkin.Default, Anchor.AutoCenter);
        settingsButton.OnClick = SettingsClicked;
        buttonPanel.AddChild(settingsButton);

        exitButton = new Button("Exit", ButtonSkin.Default, Anchor.AutoCenter);
        exitButton.OnClick = ExitClicked;
        buttonPanel.AddChild(exitButton);

        this.panel.AddChild(title);
        this.panel.AddChild(buttonPanel);
    }


    private void NewGameClicked(Entity entity) {
        SceneManager.Load(NewGameMenu.Instance);
    }

    private void ContinueGameClicked(Entity entity) {
        SceneManager.Load(LoadingScene.Instance);
        loadGame = true;
    }

    private void LoadGameClicked(Entity entity) {
		//SceneManager.Load(LoadGameMenu.Instance);
		new GameModelPersistence("test park").Load(0);
	}

    private void SettingsClicked(Entity entity) {
        SceneManager.Load(SettingsMenu.Instance);
    }

    private void ExitClicked(Entity entity) {
        Game.Instance.Exit();
    }

    protected override void DestroyUI() {
        panel = null;
        buttonPanel = null;
        title = null;
        newGameButton = null;
        continueGameButton = null;
        loadGameButton = null;
        settingsButton = null;
        exitButton = null;
    }

    public override void Update(GameTime gameTime) {
        if (loadGame) {
            SceneManager.Load(new GameScene());
            loadGame = false;
        }
    }
}
