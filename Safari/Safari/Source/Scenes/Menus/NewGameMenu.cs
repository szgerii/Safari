using Engine.Scenes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Popups;
using Engine;
using Safari.Helpers;
using Safari.Model;
using Safari.Persistence;

namespace Safari.Scenes.Menus;

public class NewGameMenu : MenuScene, IResettableSingleton {
	private static NewGameMenu instance;
	public static NewGameMenu Instance {
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
    private Panel itemPanel;
    private TextInput input;
    private Button startButton;
    private Button menuButton;
    private Panel radioPanel;
    private Header diffTitle;
    private RadioButton radioEasy;
    private RadioButton radioMedium;
    private RadioButton radioHard;
    private bool loadGame = false;

    protected override void ConstructUI() {
        panel = new Panel(new Vector2(0), PanelSkin.Default, Anchor.TopLeft);

        title = new Header("Safari", Anchor.TopCenter);
        panel.AddChild(title);

        itemPanel = new Panel(new Vector2(0.4f, 0.8f), PanelSkin.None, Anchor.Center);

        input = new TextInput(false, Anchor.AutoCenter);
        input.PlaceholderText = "Name your Safari";
        itemPanel.AddChild(input);

        radioEasy = new RadioButton("Easy", Anchor.AutoCenter);
        radioMedium = new RadioButton("Medium", Anchor.AutoCenter);
        radioHard = new RadioButton("Hard", Anchor.AutoCenter);

        radioPanel = new Panel(new Vector2(0.8f, 0.7f), PanelSkin.None, Anchor.AutoCenter);
        diffTitle = new Header("Difficulty", Anchor.AutoCenter);
        radioPanel.AddChild(diffTitle);
        radioPanel.AddChild(radioEasy);
        radioPanel.AddChild(radioMedium);
        radioPanel.AddChild(radioHard);
        itemPanel.AddChild(radioPanel);

        startButton = new Button("Start Game", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(0.6f, -1));
        startButton.OnClick = StartButtonClicked;
        itemPanel.AddChild(startButton);

        panel.AddChild(itemPanel);

        menuButton = new Button("Back to Menu", ButtonSkin.Default, Anchor.BottomRight, new Vector2(250, 60));
        menuButton.OnClick = MenuButtonClicked;
        panel.AddChild(menuButton);
    }

    private void StartButtonClicked(Entity entity) {
        if (input.Value == "") { 
            new AlertMenu("Safari name", "You must name your safari before starting a game!").Show();
            return;
        } else if(!radioEasy.Checked && !radioMedium.Checked && !radioHard.Checked) {
            new AlertMenu("Difficulty", "You must select a difficulty before starting a game!").Show();
            return;
        }
        if (GameModelPersistence.IsNameAvailable(input.Value)) {
            LoadingScene.Instance.LoadNewGame(input.Value, radioEasy.Checked ? GameDifficulty.Easy : radioMedium.Checked ? GameDifficulty.Normal : GameDifficulty.Hard);
        } else {
            new AlertMenu("Safari name", "This name is already in use!").Show();
            return;
        }
    }

    private void MenuButtonClicked(Entity entity) {
        SceneManager.Load(MainMenu.Instance);
    }

    protected override void DestroyUI() {
        panel = null;
        title = null;
        itemPanel = null;
        input = null;
        startButton = null;
        menuButton = null;
        radioPanel = null;
        diffTitle = null;
        radioEasy = null;
        radioMedium = null;
        radioHard = null;
    }
}
