using Engine;
using Engine.Graphics;
using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Scenes;

namespace Safari.Popups;

class Statusbar : PopupMenu, IUpdatable {
    private static readonly Statusbar instance = new Statusbar();
    private Panel speedButtonPanel;
    private Button pauseButton;
    private Button slowButton;
    private Button mediumButton;
    private Button fastButton;
    private Panel categoryMenuPanel;
    private CategoryMenu animals;
    private Button animalsButton;
    private CategoryMenu tiles;
    private Button tilesButton;

    public static Statusbar Instance => instance;
    public static int Height => (int)(DisplayManager.Height * 0.25);

    private Statusbar() {
        panel = new Panel(new Vector2(0, 0.25f), PanelSkin.Default, Anchor.BottomCenter);
        panel.Padding = new Vector2(20);

        speedButtonPanel = new Panel(new Vector2(0.25f, 0.1f), PanelSkin.None, Anchor.TopLeft);
        speedButtonPanel.Padding = new Vector2(0);
        speedButtonPanel.MaxSize = new Vector2(400, 280);

        Vector2 btnMaxSize = new Vector2(80, 50);

        pauseButton = new Button(" ||", ButtonSkin.Default, Anchor.TopLeft, new Vector2(0.25f, 0));
        pauseButton.MaxSize = btnMaxSize;
        slowButton = new Button(">", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.25f, 0));
        slowButton.MaxSize = btnMaxSize;
        mediumButton = new Button(">>", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.25f, 0));
        mediumButton.MaxSize = btnMaxSize;
        fastButton = new Button(">>>", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.25f, 0));
        fastButton.MaxSize = btnMaxSize;

        pauseButton.Padding = new Vector2(0);
        slowButton.Padding = new Vector2(0);
        mediumButton.Padding = new Vector2(0);
        fastButton.Padding = new Vector2(0);

        pauseButton.ToggleMode = true;
        slowButton.ToggleMode = true;
        mediumButton.ToggleMode = true;
        fastButton.ToggleMode = true;

        pauseButton.Tag = "pause-button";
        slowButton.Tag = "slow-button";
        mediumButton.Tag = "medium-button";
        fastButton.Tag = "fast-button";

        pauseButton.OnClick = adjustSpeedSettings;
        slowButton.OnClick = adjustSpeedSettings;
        mediumButton.OnClick = adjustSpeedSettings;
        fastButton.OnClick = adjustSpeedSettings;

        speedButtonPanel.AddChild(pauseButton);
        speedButtonPanel.AddChild(slowButton);
        speedButtonPanel.AddChild(mediumButton);
        speedButtonPanel.AddChild(fastButton);

        categoryMenuPanel = new Panel(new Vector2(0, 0), PanelSkin.None, Anchor.TopLeft);

        animals = new CategoryMenu("Animals");
        tiles = new CategoryMenu("Tiles");

        animalsButton = new Button("Animals", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.25f,0));
        animalsButton.Padding = new Vector2(0);
        animalsButton.OnClick = (Entity entity) => {
            animals.ToggleCategoryMenu();
        };
        animalsButton.MaxSize = new Vector2(0, 100);
        categoryMenuPanel.AddChild(animalsButton);

        tilesButton = new Button("Tiles", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.25f, 0));
        tilesButton.Padding = new Vector2(0);
        tilesButton.OnClick = (Entity entity) => {
            tiles.ToggleCategoryMenu();
        };
        tilesButton.MaxSize = new Vector2(0, 100);
        categoryMenuPanel.AddChild(tilesButton);

        panel.AddChild(categoryMenuPanel);
    }

    private void adjustSpeedSettings(Entity entity) {
        switch (entity.Tag) {
            case "pause-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.Pause();
                    pauseButton.Checked = true;
                    slowButton.Checked = false;
                    mediumButton.Checked = false;
                    fastButton.Checked = false;
                }
                break;
            case "slow-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.GameSpeed = GameSpeed.Slow;
                    pauseButton.Checked = false;
                    slowButton.Checked = true;
                    mediumButton.Checked = false;
                    fastButton.Checked = false;
                }
                break;
            case "medium-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.GameSpeed = GameSpeed.Medium;
                    pauseButton.Checked = false;
                    slowButton.Checked = false;
                    mediumButton.Checked = true;
                    fastButton.Checked = false;
                }
                break;
            case "fast-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.GameSpeed = GameSpeed.Fast;
                    pauseButton.Checked = false;
                    slowButton.Checked = false;
                    mediumButton.Checked = false;
                    fastButton.Checked = true;
                }
                break;
        }
    }

    public void Load() {
        UserInterface.Active.AddEntity(panel);
        UserInterface.Active.AddEntity(speedButtonPanel);
        adjustSpeedButtons();
    }

    private void adjustSpeedButtons() {
        if (GameScene.Active.Model.GameSpeed == GameSpeed.Paused) {
            pauseButton.Checked = true;
            slowButton.Checked = false;
            mediumButton.Checked = false;
            fastButton.Checked = false;
        } else if (GameScene.Active.Model.GameSpeed == GameSpeed.Slow) {
            pauseButton.Checked = false;
            slowButton.Checked = true;
            mediumButton.Checked = false;
            fastButton.Checked = false;
        } else if (GameScene.Active.Model.GameSpeed == GameSpeed.Medium) {
            pauseButton.Checked = false;
            slowButton.Checked = false;
            mediumButton.Checked = true;
            fastButton.Checked = false;
        } else if (GameScene.Active.Model.GameSpeed == GameSpeed.Fast) {
            pauseButton.Checked = false;
            slowButton.Checked = false;
            mediumButton.Checked = false;
            fastButton.Checked = true;
        }
    }

    public void UnLoad() {
        UserInterface.Active.RemoveEntity(panel);
        UserInterface.Active.RemoveEntity(speedButtonPanel);
    }

    public void Update(GameTime gameTime) {
    }
}
