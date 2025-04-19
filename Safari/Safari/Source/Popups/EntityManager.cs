using Engine.Objects;
using GeonBit.UI;
using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Objects.Entities;
using Safari.Objects.Entities.Animals;
using Safari.Scenes;
using Safari.Scenes.Menus;
using System;

namespace Safari.Popups;

public enum EntityManagerTab {
    AnimalTab,
    RangerTab,
    OtherTab,
    None
}

class EntityManager : PopupMenu {
    private readonly static EntityManager instance = new EntityManager();
    private bool visible;
    private EntityManagerTab currentPanel = EntityManagerTab.None;
    private Header title;
    private Panel buttonPanel;

    private Panel rangerPanel;
    private Button rangerTabBtn;

    private Button rangerDefaultTargetButton;

    private Panel rangerTopPanel;
    private Label rangerHireLabel;
    private Button rangerHirePlus;
    private Button rangerHireMinus;
    private Panel rangerListPanel;
    private int rangerTargetIndex = 0;
    bool update = false;
    int prevRangerCount;

    private Panel animalPanel;
    private Button animalTabBtn;

    private Panel otherPanel;
    private Button otherTabBtn;

    private Rectangle maskArea;

    private DefaultAnimalSelectorPopup defaultSelector = new DefaultAnimalSelectorPopup();

    public static EntityManager Instance => instance;

    private EntityManager() {
        background = null;
        visible = false;

        panel = new Panel(new Vector2(0.5f, 0.75f), PanelSkin.Default, Anchor.TopRight);
        panel.Tag = "PassiveFocus";
        panel.Padding = new Vector2(0);

        title = new Header("Entity Manager", Anchor.TopCenter, new Vector2(0, 15));
        title.Size = new Vector2(0, 0.1f);

        buttonPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        buttonPanel.Padding = new Vector2(15);

        Vector2 maxSize = new Vector2(0.3f, 80);

        rangerTabBtn = new Button("Rangers", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.3f, 0));
        rangerTabBtn.OnClick = switchToRangerTab;
        rangerTabBtn.Padding = new Vector2(0);
        rangerTabBtn.MaxSize = maxSize;

        animalTabBtn = new Button("Animals", ButtonSkin.Default, Anchor.Center, new Vector2(0.3f, 0));
        animalTabBtn.OnClick = switchToAnimalTab;
        animalTabBtn.Padding = new Vector2(0);
        animalTabBtn.MaxSize = maxSize;

        otherTabBtn = new Button("Others", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, 0));
        otherTabBtn.OnClick = switchToOtherTab;
        otherTabBtn.Padding = new Vector2(0);
        otherTabBtn.MaxSize = maxSize;

        buttonPanel.AddChild(rangerTabBtn);
        buttonPanel.AddChild(animalTabBtn);
        buttonPanel.AddChild(otherTabBtn);

        Vector2 tabPanelSize = new Vector2(0, 0.7f);

        #region RANGER_TAB
        rangerPanel = new Panel(tabPanelSize, PanelSkin.Default, Anchor.BottomLeft, new Vector2(0));
        rangerPanel.Padding = new Vector2(15);

        rangerTopPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.TopLeft);
        rangerTopPanel.Padding = new Vector2(0);

        rangerDefaultTargetButton = new Button("Default Target", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.5f, 0));
        rangerDefaultTargetButton.Padding = new Vector2(0);
        rangerDefaultTargetButton.OnClick = (GeonBit.UI.Entities.Entity entity) => {
            if (!defaultSelector.Visible) {
                defaultSelector.Show();
            }
        };

        Panel hirePanel = new Panel(new Vector2(0.5f, 0), PanelSkin.None, Anchor.AutoInline);
        hirePanel.Padding = new Vector2(0);

        Panel rangerCountTextPanel = new Panel(new Vector2(0.2f, 0), PanelSkin.None, Anchor.Center);
        rangerCountTextPanel.Padding = new Vector2(0);

        rangerHireLabel = new Label(GameScene.Active.Model.RangerCount.ToString(), Anchor.Center, new Vector2(0));
        rangerHireLabel.Padding = new Vector2(0);
        rangerHireLabel.AlignToCenter = true;

        rangerHireMinus = new Button("-", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.4f, 0));
        rangerHireMinus.Padding = new Vector2(0);
        rangerHireMinus.MaxSize = new Vector2(100, 100);

        rangerHirePlus = new Button("+", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.4f, 0));
        rangerHirePlus.Padding = new Vector2(0);
        rangerHirePlus.MaxSize = new Vector2(100, 100);

        rangerListPanel = new Panel(new Vector2(0, 0.8f), PanelSkin.Default, Anchor.BottomLeft);
        rangerListPanel.Padding = new Vector2(15);
        rangerListPanel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;

        rangerCountTextPanel.AddChild(rangerHireLabel);

        hirePanel.AddChild(rangerHireMinus);
        hirePanel.AddChild(rangerCountTextPanel);
        hirePanel.AddChild(rangerHirePlus);

        rangerTopPanel.AddChild(rangerDefaultTargetButton);
        rangerTopPanel.AddChild(hirePanel);
        rangerPanel.AddChild(rangerTopPanel);
        rangerPanel.AddChild(rangerListPanel);
        #endregion

        #region ANIMAL_TAB
        #endregion

        #region OTHER_TAB
        #endregion

        panel.AddChild(title);
        panel.AddChild(buttonPanel);

        updateCurrentTab();

        SettingsMenu.ScaleChanged += scaleText;
    }

    public void Toggle() {
        if (visible) {
            Hide();
            visible = false;
        } else {
            Show();
            visible = true;
        }
    }

    private void removeCurrentTab() {
        if (currentPanel == EntityManagerTab.RangerTab) {
            panel.RemoveChild(rangerPanel);
        } else if (currentPanel == EntityManagerTab.AnimalTab) {
            panel.RemoveChild(animalPanel);
        } else if (currentPanel == EntityManagerTab.OtherTab) {
            panel.RemoveChild(otherPanel);
        }
    }

    private void switchToRangerTab(GeonBit.UI.Entities.Entity entity) {
        if (currentPanel != EntityManagerTab.RangerTab) {
            removeCurrentTab();
            currentPanel = EntityManagerTab.RangerTab;
            panel.AddChild(rangerPanel);
        }
        updateCurrentTab();
    }

    private void switchToAnimalTab(GeonBit.UI.Entities.Entity entity) {
        if (currentPanel != EntityManagerTab.AnimalTab) {
            removeCurrentTab();
            currentPanel = EntityManagerTab.AnimalTab;
            panel.AddChild(animalPanel);
        }
        updateAnimalList();
    }

    private void switchToOtherTab(GeonBit.UI.Entities.Entity entity) {
        if (currentPanel != EntityManagerTab.OtherTab) {
            removeCurrentTab();
            currentPanel = EntityManagerTab.OtherTab;
            panel.AddChild(otherPanel);
        }
    }

    public override void Show() {
        rangerHireMinus.OnClick += rangerMinusBtn;
        rangerHirePlus.OnClick += rangerPlusBtn;
        updateCurrentTab();
        base.Show();
        maskArea = this.panel.CalcDestRect();
        GameScene.Active.Model.Level.maskedAreas.Add(maskArea);
    }

    public override void Hide() {
        rangerHireMinus.OnClick -= rangerMinusBtn;
        rangerHirePlus.OnClick -= rangerPlusBtn;
        base.Hide();
		GameScene.Active.Model.Level.maskedAreas.Remove(maskArea);
    }

    public void Unload() {
        visible = false;
        removeCurrentTab();
        currentPanel = EntityManagerTab.None;
        if (defaultSelector.Visible) {
            defaultSelector.Hide();
        }
        Hide();
    }

    public void Load() {
        visible = false;
        Ranger.DefaultTarget = null;
        rangerDefaultTargetButton.ButtonParagraph.Text = "Default Target";
    }

    public void SetDefaultTarget(AnimalSpecies animal) {
        Ranger.DefaultTarget = animal;
        rangerDefaultTargetButton.ButtonParagraph.Text = animal.GetDisplayName() ?? "Default Target";
    }

    private void scaleText(object sender, EventArgs e) {
        rangerHireLabel.Scale = SettingsMenu.Scale;
        rangerHireMinus.ButtonParagraph.Scale = SettingsMenu.Scale;
        rangerHirePlus.ButtonParagraph.Scale = SettingsMenu.Scale;
    }

    private void rangerMinusBtn(GeonBit.UI.Entities.Entity entity) {
        if (GameScene.Active.Model.RangerCount == 0) {
            new AlertMenu("Ranger count", "You don't have any more rangers to fire").Show();
            return;
        }
        foreach (Objects.Entities.Entity ent in Ranger.ActiveEntities) {
            if (ent is Ranger) {
                ((Ranger)ent).Fire();
                break;
            }
        }
        update = true;
        updateCurrentTab();
    }

    private void rangerPlusBtn(GeonBit.UI.Entities.Entity entity) {
        Ranger temp = new Ranger(new Vector2((GameScene.Active.Model.Level.MapWidth / 2) * GameScene.Active.Model.Level.TileSize, (GameScene.Active.Model.Level.MapHeight / 2) * GameScene.Active.Model.Level.TileSize)) {
            TargetSpecies = null
        };
        rangerTargetIndex = (rangerTargetIndex + 1) % Enum.GetValues(typeof(AnimalSpecies)).Length;
        Game.AddObject(temp);
        rangerHireLabel.Text = GameScene.Active.Model.RangerCount.ToString();
        update = true;
        updateCurrentTab();
        DebugConsole.Instance.Write("asd");
    }

    private void updateCurrentTab() {
        if (currentPanel == EntityManagerTab.RangerTab) {
            updateRangerList();
        } else if (currentPanel == EntityManagerTab.AnimalTab) {
            updateAnimalList();
        } else if (currentPanel == EntityManagerTab.OtherTab) {
            updateOtherList();
        }
    }

    private void updateOtherList() {
        //throw new NotImplementedException();
    }

    private void updateAnimalList() {
        //throw new NotImplementedException();
    }

    private void updateRangerList() {
        int count = 0;
        rangerListPanel.ClearChildren();
        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);

        StyleProperty deleteBase = new StyleProperty(Color.Red);
        StyleProperty deleteHover = new StyleProperty(Color.DarkRed);
        StyleProperty deleteClick = new StyleProperty(Color.IndianRed);

        foreach (Objects.Entities.Entity ent in Ranger.ActiveEntities) {
            if (ent is Ranger) {
                Panel tempPanel = new Panel(new Vector2(0, 0.25f), PanelSkin.None, Anchor.Auto);
                Panel tempControllerPanel = new Panel(new Vector2(0.7f, 0), PanelSkin.Simple, Anchor.CenterLeft);

                tempPanel.Padding = new Vector2(0);
                tempControllerPanel.Padding = new Vector2(0);
                tempControllerPanel.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                    new RangerControllerMenu((Ranger)ent).Show();
                    Camera.Active.GetComponent<CameraControllerCmp>().CenterOnPosition(ent.Position);
                    Toggle();
                };

                Label tempLabel = new Label("Ranger#" + (++count).ToString(), Anchor.CenterLeft, new Vector2(0.5f, 0), new Vector2(15, 0));
                tempLabel.ClickThrough = true;
                tempLabel.Scale = SettingsMenu.Scale;

                Button tempButtonFire = new Button("Fire", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, 0));
                tempButtonFire.Padding = new Vector2(0);
                tempButtonFire.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                    ((Ranger)ent).Fire();
                    updateCurrentTab();
                    update = true;
                };
                tempButtonFire.SetStyleProperty("FillColor", deleteBase, EntityState.Default);
                tempButtonFire.SetStyleProperty("FillColor", deleteHover, EntityState.MouseHover);
                tempButtonFire.SetStyleProperty("FillColor", deleteClick, EntityState.MouseDown);

                tempControllerPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
                tempControllerPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);

                tempControllerPanel.AddChild(tempLabel);
                tempPanel.AddChild(tempControllerPanel);
                tempPanel.AddChild(tempButtonFire);
                rangerListPanel.AddChild(tempPanel);
            }
        }
    }

    public override void Update(GameTime gameTime) {
        if (!visible) {
            return;
        }
        if (currentPanel == EntityManagerTab.RangerTab) {
            rangerHireLabel.Text = GameScene.Active.Model.RangerCount.ToString();
        }
        if (update) {
            update = false;
            updateCurrentTab();
        }
        if(prevRangerCount != GameScene.Active.Model.RangerCount) {
            updateRangerList();
        }
        prevRangerCount = GameScene.Active.Model.RangerCount;
    }
}
