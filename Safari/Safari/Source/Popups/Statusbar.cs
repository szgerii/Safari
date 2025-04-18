using Engine;
using Engine.Graphics;
using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Objects.Entities.Tourists;
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

    private Panel indicatorPanel;

    private Panel moneyPanel;
    private Label moneyText;
    private int moneyPrev;
    private int moneyCurr;

    private Panel ratingPanel;
    private Label ratingText;
    private float ratingPrev;
    private float ratingCurr;

    private Panel carnivorePanel;
    private Label carnivoreText;
    private int carnivorePrev;
    private int carnivoreCurr;

    private Panel herbivorePanel;
    private Label herbivoreText;
    private int herbivorePrev;
    private int herbivoreCurr;

    private Button entityManagerButton;

    public static Statusbar Instance => instance;

    public static int Height => Instance.panel.GetActualDestRect().Height;

    private Statusbar() {
        background = null;

        panel = new Panel(new Vector2(0, 0.25f), PanelSkin.Default, Anchor.BottomCenter);
        panel.Padding = new Vector2(0);
        panel.Tag = "PassiveFocus";

        #region SPEED_BUTTONS
        speedButtonPanel = new Panel(new Vector2(0.25f, 0.5f), PanelSkin.None, Anchor.TopLeft);
        speedButtonPanel.Padding = new Vector2(0);
        speedButtonPanel.Offset = new Vector2(20);
        //speedButtonPanel.MaxSize = new Vector2(400, 280);
        speedButtonPanel.Tag = "PassiveFocus";

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
        #endregion

        entityManagerButton = new Button("Entity Manager", ButtonSkin.Default, Anchor.TopRight, new Vector2(0.3f, 0.3f), new Vector2(20));
        entityManagerButton.Padding = new Vector2(0);
        entityManagerButton.OnClick = (Entity entity) => {
            EntityManager.Instance.Toggle();
        };
        entityManagerButton.MaxSize = new Vector2(400, 0.3f);
        panel.AddChild(entityManagerButton);

        indicatorPanel = new Panel(new Vector2(0, 0.5f), PanelSkin.None, Anchor.BottomLeft);
        indicatorPanel.Padding = new Vector2(0);

        Vector2 panelsize = new Vector2(0.25f,0);

        #region INDICATORS
        moneyPanel = new Panel(panelsize, PanelSkin.Default, Anchor.CenterLeft);
        ratingPanel = new Panel(panelsize, PanelSkin.Default, Anchor.AutoInline);
        carnivorePanel = new Panel(panelsize, PanelSkin.Default, Anchor.AutoInline);
        herbivorePanel = new Panel(panelsize, PanelSkin.Default, Anchor.AutoInline);

        moneyText = new Label("Money:", Anchor.CenterLeft);
        ratingText = new Label("Rating:", Anchor.CenterLeft);
        carnivoreText = new Label("Carnivores:", Anchor.CenterLeft);
        herbivoreText = new Label("Herbivores:", Anchor.CenterLeft);

        moneyPanel.AddChild(moneyText);
        ratingPanel.AddChild(ratingText);
        carnivorePanel.AddChild(carnivoreText);
        herbivorePanel.AddChild(herbivoreText);

        indicatorPanel.AddChild(moneyPanel);
        indicatorPanel.AddChild(ratingPanel);
        indicatorPanel.AddChild(carnivorePanel);
        indicatorPanel.AddChild(herbivorePanel);
        #endregion

        panel.AddChild(indicatorPanel);
        panel.AddChild(speedButtonPanel);
    }

    private void adjustSpeedSettings(Entity entity) {
        switch (entity.Tag) {
            case "pause-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.Pause();
                }
                break;
            case "slow-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.GameSpeed = GameSpeed.Slow;
                }
                break;
            case "medium-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.GameSpeed = GameSpeed.Medium;
                }
                break;
            case "fast-button":
                if (SceneManager.Active is GameScene) {
                    GameModel model = GameScene.Active.Model;
                    model.GameSpeed = GameSpeed.Fast;
                }
                break;
        }
        adjustSpeedButtons();
    }

    public void Load() {
        UserInterface.Active.AddEntity(panel);
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
    }

    public override void Update(GameTime gameTime) {
        moneyPrev = moneyCurr;
        moneyCurr = GameScene.Active.Model.Funds;
        moneyText.Text = "Money: " + moneyCurr;

        ratingPrev = ratingCurr;
        ratingCurr = (float)Tourist.AvgRating;
        ratingText.Text = "Rating: " + ratingCurr.ToString("0.00");

        carnivorePrev = carnivoreCurr;
        carnivoreCurr = GameScene.Active.Model.CarnivoreCount;
        carnivoreText.Text = "Carnivores: " + carnivoreCurr;

        herbivorePrev = herbivoreCurr;
        herbivoreCurr = GameScene.Active.Model.HerbivoreCount;
        herbivoreText.Text = "Herbivores: " + herbivoreCurr;
    }
}
