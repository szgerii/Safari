using Engine;
using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Objects.Entities.Tourists;
using Safari.Scenes;
using Safari.Scenes.Menus;
using System;

namespace Safari.Popups;

class Statusbar : PopupMenu, IUpdatable {
    private static readonly Statusbar instance = new Statusbar();

    private bool visible;

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
    private double moneyCurr;

    private Panel ratingPanel;
    private Label ratingText;
    private double ratingCurr;

    private Panel carnivorePanel;
    private Label carnivoreText;
    private int carnivoreCurr;

    private Panel herbivorePanel;
    private Label herbivoreText;
    private int herbivoreCurr;

    private Panel winDaysPanel;
    private Label winDaysText;

    private Button entityManagerButton;

    private Rectangle maskArea;
    public static Statusbar Instance => instance;

    private Statusbar() {
        background = null;

        panel = new Panel(new Vector2(0, 0.25f), PanelSkin.Default, Anchor.BottomCenter);
        panel.Padding = new Vector2(0);
        panel.Tag = "PassiveFocus";
        panel.MaxSize = new Vector2(0, 200);

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

        pauseButton.OnClick = AdjustSpeedSettings;
        slowButton.OnClick = AdjustSpeedSettings;
        mediumButton.OnClick = AdjustSpeedSettings;
        fastButton.OnClick = AdjustSpeedSettings;

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

        Vector2 panelSize = new Vector2(0.2f, 0);
        Vector2 paddingSize = new Vector2(0, 20);

        #region INDICATORS

        moneyPanel = new Panel(new Vector2(0.25f, 0), PanelSkin.Default, Anchor.CenterLeft);
        moneyPanel.Padding = paddingSize;
        ratingPanel = new Panel(new Vector2(0.15f, 0), PanelSkin.Default, Anchor.AutoInline);
        ratingPanel.Padding = paddingSize;
        carnivorePanel = new Panel(panelSize, PanelSkin.Default, Anchor.AutoInline);
        carnivorePanel.Padding = paddingSize;
        herbivorePanel = new Panel(panelSize, PanelSkin.Default, Anchor.AutoInline);
        herbivorePanel.Padding = paddingSize;
        winDaysPanel = new Panel(panelSize, PanelSkin.Default, Anchor.AutoInline);
        winDaysPanel.Padding = paddingSize;

        moneyText = new Label("Money:", Anchor.CenterLeft);
        moneyText.Offset = new Vector2(15, 0);
        ratingText = new Label("Rating:", Anchor.CenterLeft);
        ratingText.Offset = new Vector2(15, 0);
        carnivoreText = new Label("Carnivores:", Anchor.CenterLeft);
        carnivoreText.Offset = new Vector2(15, 0);
        herbivoreText = new Label("Herbivores:", Anchor.CenterLeft);
        herbivoreText.Offset = new Vector2(15, 0);
        winDaysText = new Label("Winning:", Anchor.CenterLeft);
        winDaysText.Offset = new Vector2(15, 0);

        moneyPanel.AddChild(moneyText);
        ratingPanel.AddChild(ratingText);
        carnivorePanel.AddChild(carnivoreText);
        herbivorePanel.AddChild(herbivoreText);
        winDaysPanel.AddChild(winDaysText);

        indicatorPanel.AddChild(moneyPanel);
        indicatorPanel.AddChild(ratingPanel);
        indicatorPanel.AddChild(carnivorePanel);
        indicatorPanel.AddChild(herbivorePanel);
        indicatorPanel.AddChild(winDaysPanel);
        #endregion

        SettingsMenu.ScaleChanged += ScaleText;

        panel.AddChild(indicatorPanel);
        panel.AddChild(speedButtonPanel);
    }

    private void AdjustSpeedSettings(Entity entity) {
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
        AdjustSpeedButtons();
    }

    public void Load() {
        visible = true;
        UserInterface.Active.AddEntity(panel);
        AdjustSpeedButtons();
        ScaleText(null, EventArgs.Empty);

        maskArea = panel.CalcDestRect();
        GameScene.Active.MaskedAreas.Add(maskArea);
    }

    public void Unload() {
        visible = false;
        if (panel.Parent != null) {
            UserInterface.Active.RemoveEntity(panel);
            GameScene.Active.MaskedAreas.Remove(maskArea);
        }
    }

    public void Toggle() {
        if (visible) {
            visible = false;
            UserInterface.Active.RemoveEntity(panel);
            GameScene.Active.MaskedAreas.Remove(maskArea);
        } else {
            visible = true;
            UserInterface.Active.AddEntity(panel);
            maskArea = panel.CalcDestRect();
            GameScene.Active.MaskedAreas.Add(maskArea);
            panel.SendToBack();
        }
    }

    private void AdjustSpeedButtons() {
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

    public void SetSpeed(GameSpeed speed) {
        if (SceneManager.Active is not GameScene) {
            return;
        }
        GameModel model = GameScene.Active.Model;
        if (speed == GameSpeed.Paused) {
            if (model.GameSpeed == GameSpeed.Paused) {
                model.Resume();
            } else if (model.GameSpeed != GameSpeed.Paused) {
                model.Pause();
            }
        } else if (speed == GameSpeed.Slow) {
            model.GameSpeed = GameSpeed.Slow;
        } else if (speed == GameSpeed.Medium) {
            model.GameSpeed = GameSpeed.Medium;
        } else if (speed == GameSpeed.Fast) {
            model.GameSpeed = GameSpeed.Fast;
        }
        AdjustSpeedButtons();
    }

    private void ScaleText(object sender, EventArgs e) {
        moneyText.Scale = SettingsMenu.Scale;
        ratingText.Scale = SettingsMenu.Scale;
        carnivoreText.Scale = SettingsMenu.Scale;
        herbivoreText.Scale = SettingsMenu.Scale;
        winDaysText.Scale = SettingsMenu.Scale;
    }

    public override void Update(GameTime gameTime) {
        moneyCurr = GameScene.Active.Model.Funds;
        moneyText.Text = "Money: " +
            (moneyCurr >= 10000 ? (moneyCurr / 1000d) + "K" : moneyCurr.ToString()) + "/" +
            (GameScene.Active.Model.WinCriteriaFunds >= 10000 ? (GameScene.Active.Model.WinCriteriaFunds / 1000d) + "K" : GameScene.Active.Model.WinCriteriaFunds.ToString());

        ratingCurr = Tourist.AvgRating;
        ratingText.Text = "Rating: " + ratingCurr.ToString("0.00");

        carnivoreCurr = GameScene.Active.Model.CarnivoreCount;
        carnivoreText.Text = "Carnivores: " + carnivoreCurr + "/" + GameScene.Active.Model.WinCriteriaCarn;

        herbivoreCurr = GameScene.Active.Model.HerbivoreCount;
        herbivoreText.Text = "Herbivores: " + herbivoreCurr + "/" + GameScene.Active.Model.WinCriteriaHerb;

        winDaysText.Text = GameScene.Active.Model.WinTimerRunning ?
            "Winning in:\n" + GameScene.Active.Model.WinTimerDays.ToString("0.00") + "/" + GameScene.Active.Model.WinCriteriaDays + " days"
            : "Winning in: -";
    }
}
