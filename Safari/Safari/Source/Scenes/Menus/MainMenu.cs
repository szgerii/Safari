using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Popups;
using System;

namespace Safari.Scenes.Menus;
class MainMenu : MenuScene {
    private readonly static MainMenu active = new MainMenu();
    private Header title;
    private Panel buttonPanel;
    private Button newGameButton;
    private Button continueGameButton;
    private Button loadGameButton;
    private Button settingsButton;
    private Button exitButton;

    public static MainMenu Active => active;

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
        SceneManager.Load(NewGameMenu.Active);
    }

    private void ContinueGameClicked(Entity entity) {
        SceneManager.Load(new GameScene());
    }

    private void LoadGameClicked(Entity entity) {
        SceneManager.Load(LoadGameMenu.Active);
    }

    private void SettingsClicked(Entity entity) {
        SceneManager.Load(SettingsMenu.Active);
    }

    private void ExitClicked(Entity entity) {
        Game.Instance.Exit();
    }

    protected override void DestroyUI() {
        this.panel = null;
        buttonPanel = null;
        title = null;
        newGameButton = null;
        continueGameButton = null;
        loadGameButton = null;
        settingsButton = null;
        exitButton = null;
    }
}
