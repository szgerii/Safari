using Engine;
using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Helpers;
using Safari.Model;
using Safari.Model.Entities.Tourists;
using Safari.Scenes;
using Safari.Scenes.Menus;
using System;

namespace Safari.Popups;

public class Statusbar : PopupMenu, IUpdatable, IResettableSingleton {
    private static Statusbar instance;
	public static Statusbar Instance {
		get {
			instance ??= new();
			return instance;
		}
	}
    public static void ResetSingleton() {
        instance?.Hide();
        instance = null;
    }

    private bool visible;

    private readonly Panel speedButtonPanel;
    private readonly Button pauseButton;
    private readonly Button slowButton;
    private readonly Button mediumButton;
    private readonly Button fastButton;

    private readonly Panel shopPanel;

    private readonly AnimalMenu animals;
    private readonly Button animalsButton;
    private readonly TileMenu tiles;
    private readonly Button tilesButton;
    private readonly OtherMenu others;
    private readonly Button othersButton;

    private readonly Panel indicatorPanel;

    private readonly Panel moneyPanel;
    private readonly Label moneyText;
    private double moneyCurr;

    private readonly Panel ratingPanel;
    private readonly Label ratingText;
    private double ratingCurr;

    private readonly Panel carnivorePanel;
    private readonly Label carnivoreText;
    private int carnivoreCurr;

    private readonly Panel herbivorePanel;
    private readonly Label herbivoreText;
    private int herbivoreCurr;

    private readonly Panel winDaysPanel;
    private readonly Label winDaysText;

    private readonly Button entityManagerButton;

    public Rectangle Size => panel.CalcDestRect();

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

        entityManagerButton = new Button("Entity Manager", ButtonSkin.Default, Anchor.TopRight, new Vector2(0.25f, 0.3f), new Vector2(20));
        entityManagerButton.Padding = new Vector2(0);
        entityManagerButton.OnClick = (Entity entity) => {
            EntityManager.Instance.Toggle();
			animals.Hide();
			tiles.Hide();
			others.Hide();
		};
        entityManagerButton.MaxSize = new Vector2(400, 0.3f);
        panel.AddChild(entityManagerButton);

        indicatorPanel = new Panel(new Vector2(0, 0.5f), PanelSkin.None, Anchor.BottomLeft);
        indicatorPanel.Padding = new Vector2(0);

        Vector2 panelSize = new Vector2(0.2f, 0);
        Vector2 paddingSize = new Vector2(0, 20);

        #region SHOP
        shopPanel = new Panel(new Vector2(0.45f, 0.3f), PanelSkin.None, Anchor.TopCenter, new Vector2(0, 20));
        shopPanel.Padding = new Vector2(0);

        animalsButton = new Button("Animal", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.3f, 0));
        animalsButton.Padding = new Vector2(0);
        animalsButton.OnClick = AnimalButton;
        shopPanel.AddChild(animalsButton);
        animals = new AnimalMenu();

        tilesButton = new Button("Tile", ButtonSkin.Default, Anchor.Center, new Vector2(0.3f, 0));
        tilesButton.Padding = new Vector2(0);
        tilesButton.OnClick = TileButton;
        shopPanel.AddChild(tilesButton);
        tiles = new TileMenu();

        othersButton = new Button("Other", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, 0));
        othersButton.Padding = new Vector2(0);
        othersButton.OnClick = OtherButton;
        shopPanel.AddChild(othersButton);
        others = new OtherMenu();
        #endregion

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

        panel.AddChild(shopPanel);
        panel.AddChild(indicatorPanel);
        panel.AddChild(speedButtonPanel);
    }

    private void AnimalButton(Entity entity) {
        EntityManager.Instance.Hide();
        EntityControllerMenu.Active?.Hide();
        tiles.Hide();
        others.Hide();
        animals.ToggleCategoryMenu();
    }

    private void TileButton(Entity entity) {
        EntityManager.Instance.Hide();
        EntityControllerMenu.Active?.Hide();
        animals.Hide();
        others.Hide();
        tiles.ToggleCategoryMenu();
    }

    private void OtherButton(Entity entity) {
        EntityManager.Instance.Hide();
        EntityControllerMenu.Active?.Hide();
        animals.Hide();
        tiles.Hide();
        others.ToggleCategoryMenu();
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
        base.Show();
        EntityManager.Instance.Load();
        AdjustSpeedButtons();
        ScaleText(null, EventArgs.Empty);
    }

    public void Unload() {
        visible = false;
        base.Hide();
        EntityManager.Instance.Unload();
        animals.Hide();
        tiles.Hide();
        others.Hide();
    }

    /// <summary>
    /// Toggles Statusbar and children's visibility.
    /// </summary>
    public void Toggle() {
        if (visible) {
            Unload();
        } else {
            Load();
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
        EntityManager.Instance.Update(gameTime);
        tiles.Update(gameTime);
        moneyCurr = GameScene.Active.Model.Funds;
        moneyText.Text = "Money: " +
            (moneyCurr >= 10000 ? ((moneyCurr / 1000d)).ToString("0.00") + "K" : moneyCurr.ToString()) + "/" +
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
