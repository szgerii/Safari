using Engine.Objects;
using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Model.Entities.Tourists;
using Safari.Scenes;
using Safari.Scenes.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

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
    private readonly Header title;
    private readonly Panel buttonPanel;

    private readonly Panel rangerPanel;
    private readonly Button rangerTabBtn;
    private readonly Button rangerDefaultTargetButton;
    private readonly Panel rangerTopPanel;
    private readonly Label rangerHireLabel;
    private readonly Button rangerHirePlus;
    private readonly Button rangerHireMinus;
    private readonly Panel rangerListPanel;
    private int rangerTargetIndex = 0;
    bool update = false;
    int prevRangerCount;

    private readonly Panel animalPanel;
    private readonly Button animalTabBtn;
    private readonly Panel animalListPanel;

    private readonly Panel otherPanel;
    private readonly Button otherTabBtn;
    private readonly Panel jeepRentFeePanel;
    private readonly Label jeepRentFeeLabel;
    private readonly Label jeepRentFeeDisplayLabel;
    private readonly Panel jeepRentFeeDisplayPanel;
    private readonly Button jeepRentFeeMinusButton;
    private readonly Button jeepRentFeePlusButton;

    private Rectangle maskArea;

    private readonly DefaultAnimalSelectorPopup defaultSelector = DefaultAnimalSelectorPopup.Instance;

    public static EntityManager Instance => instance;

    private EntityManager() {
        background = null;
        visible = false;

        panel = new Panel(new Vector2(0.5f, 0.75f), PanelSkin.Default, Anchor.TopRight, new Vector2(16));
        panel.Tag = "PassiveFocus";
        panel.Padding = new Vector2(0);

        title = new Header("Entity Manager", Anchor.TopCenter, new Vector2(0, 15));
        title.Size = new Vector2(0, 0.1f);

        #region BUTTON_PANEL
        buttonPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.None, Anchor.AutoCenter);
        buttonPanel.Padding = new Vector2(15);

        Vector2 maxSize = new Vector2(0.3f, 80);

        rangerTabBtn = new Button("Rangers", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.3f, 0));
        rangerTabBtn.Padding = new Vector2(0);
        rangerTabBtn.MaxSize = maxSize;

        animalTabBtn = new Button("Animals", ButtonSkin.Default, Anchor.Center, new Vector2(0.3f, 0));
        animalTabBtn.Padding = new Vector2(0);
        animalTabBtn.MaxSize = maxSize;

        otherTabBtn = new Button("Others", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, 0));
        otherTabBtn.Padding = new Vector2(0);
        otherTabBtn.MaxSize = maxSize;

        buttonPanel.AddChild(rangerTabBtn);
        buttonPanel.AddChild(animalTabBtn);
        buttonPanel.AddChild(otherTabBtn);
        #endregion

        Vector2 tabPanelSize = new Vector2(0, 0.7f);

        #region RANGER_TAB
        rangerPanel = new Panel(tabPanelSize, PanelSkin.None, Anchor.BottomLeft, new Vector2(0));
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

        rangerListPanel = new Panel(new Vector2(0, 0.8f), PanelSkin.None, Anchor.BottomLeft);
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
        animalPanel = new Panel(tabPanelSize, PanelSkin.None, Anchor.BottomLeft, new Vector2(0));
        animalListPanel = new Panel(new Vector2(0, 0), PanelSkin.Default, Anchor.BottomLeft);
        animalListPanel.Padding = new Vector2(15);
        animalListPanel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;

        animalPanel.AddChild(animalListPanel);
        #endregion

        #region OTHER_TAB
        otherPanel = new Panel(tabPanelSize, PanelSkin.None, Anchor.BottomLeft, new Vector2(0));

        jeepRentFeePanel = new Panel(new Vector2(0, 0.3f), PanelSkin.Simple, Anchor.TopCenter);
        jeepRentFeePanel.Padding = new Vector2(15);

        jeepRentFeeLabel = new Label("Jeep rent: ", Anchor.CenterLeft, new Vector2(0.3f, 0));
        jeepRentFeeLabel.Padding = new Vector2(0);

        jeepRentFeeDisplayLabel = new Label(Jeep.RentFee.ToString(), Anchor.Center, new Vector2(0));
        jeepRentFeeDisplayLabel.Padding = new Vector2(0);
        jeepRentFeeDisplayLabel.AlignToCenter = true;

        jeepRentFeeDisplayPanel = new Panel(new Vector2(0.2f, 0), PanelSkin.None, Anchor.AutoInline);
        jeepRentFeeDisplayPanel.Padding = new Vector2(0);

        jeepRentFeeMinusButton = new Button("-", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.25f, 0));
        jeepRentFeeMinusButton.Padding = new Vector2(0);
        jeepRentFeeMinusButton.MaxSize = new Vector2(100, 100);

        jeepRentFeePlusButton = new Button("+", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.25f, 0));
        jeepRentFeePlusButton.Padding = new Vector2(0);
        jeepRentFeePlusButton.MaxSize = new Vector2(100, 100);

        jeepRentFeeDisplayPanel.AddChild(jeepRentFeeDisplayLabel);
        jeepRentFeePanel.AddChild(jeepRentFeeLabel);
        jeepRentFeePanel.AddChild(jeepRentFeeMinusButton);
        jeepRentFeePanel.AddChild(jeepRentFeeDisplayPanel);
        jeepRentFeePanel.AddChild(jeepRentFeePlusButton);
        otherPanel.AddChild(jeepRentFeePanel);
        #endregion

        panel.AddChild(title);
        panel.AddChild(buttonPanel);
        update = true;

        SettingsMenu.ScaleChanged += ScaleText;
    }

    public void Toggle() {
        if (visible) {
            Hide();
        } else {
            Show();
        }
    }

    private void RemoveCurrentTab() {
        if (currentPanel == EntityManagerTab.RangerTab) {
            panel.RemoveChild(rangerPanel);
        } else if (currentPanel == EntityManagerTab.AnimalTab) {
            panel.RemoveChild(animalPanel);
        } else if (currentPanel == EntityManagerTab.OtherTab) {
            panel.RemoveChild(otherPanel);
        }
    }

    private void LoadTab(EntityManagerTab tab) {
        if (tab == EntityManagerTab.RangerTab) {
            panel.AddChild(rangerPanel);
            currentPanel = EntityManagerTab.RangerTab;
        } else if (tab == EntityManagerTab.AnimalTab) {
            panel.AddChild(animalPanel);
            currentPanel = EntityManagerTab.AnimalTab;
        } else if (tab == EntityManagerTab.OtherTab) {
            panel.AddChild(otherPanel);
            currentPanel = EntityManagerTab.OtherTab;
        }
    }

    private void SwitchToTab(EntityManagerTab tab) {
        if (currentPanel == tab) {
            return;
        }
        RemoveCurrentTab();
        LoadTab(tab);
        update = true;
    }

    public override void Show() {
        visible = true;
        rangerHireMinus.OnClick = RangerMinusBtn;
        rangerHirePlus.OnClick = RangerPlusBtn;

        jeepRentFeeMinusButton.OnClick = JeepMinusBtn;
        jeepRentFeePlusButton.OnClick = JeepPlusBtn;

        rangerTabBtn.OnClick = (GeonBit.UI.Entities.Entity entity) => SwitchToTab(EntityManagerTab.RangerTab);
        animalTabBtn.OnClick = (GeonBit.UI.Entities.Entity entity) => SwitchToTab(EntityManagerTab.AnimalTab);
        otherTabBtn.OnClick = (GeonBit.UI.Entities.Entity entity) => SwitchToTab(EntityManagerTab.OtherTab);

        update = true;
        base.Show();
        maskArea = this.panel.CalcDestRect();
        GameScene.Active.MaskedAreas.Add(maskArea);
    }

    public override void Hide() {
        visible = false;
        rangerHireMinus.OnClick = null;
        rangerHirePlus.OnClick = null;

        jeepRentFeeMinusButton.OnClick = null;
        jeepRentFeePlusButton.OnClick = null;

        rangerTabBtn.OnClick = null;
        animalTabBtn.OnClick = null;
        otherTabBtn.OnClick = null;

        if (panel.Parent != null) {
            base.Hide();
        }
        GameScene.Active.MaskedAreas.Remove(maskArea);
    }

    public void Unload() {
        visible = false;
        RemoveCurrentTab();
        currentPanel = EntityManagerTab.None;
        if (DefaultAnimalSelectorPopup.Showing) {
            defaultSelector.Hide();
        }
        if (panel.Parent != null) {
            Hide();
        }
    }

    public void Load() {
        visible = false;
        jeepRentFeeDisplayLabel.Text = Jeep.RentFee.ToString();
		rangerDefaultTargetButton.ButtonParagraph.Text = Ranger.DefaultTarget == null ? "Default Target" : Ranger.DefaultTarget.Value.GetDisplayName();
		if (DefaultAnimalSelectorPopup.Showing) {
            defaultSelector.Hide();
        }
    }

    private void JeepMinusBtn(GeonBit.UI.Entities.Entity entity) {
        if (Jeep.RentFee == 0) {
            new AlertMenu("Jeep rent fee", "You can't set the rent fee below zero").Show();
            return;
        }
        Jeep.RentFee -= 50;
        jeepRentFeeDisplayLabel.Text = Jeep.RentFee.ToString();
    }

    private void JeepPlusBtn(GeonBit.UI.Entities.Entity entity) {
        Jeep.RentFee += 50;
        jeepRentFeeDisplayLabel.Text = Jeep.RentFee.ToString();
    }

    public void SetDefaultTarget(AnimalSpecies animal) {
        Ranger.DefaultTarget = animal;
        rangerDefaultTargetButton.ButtonParagraph.Text = animal.GetDisplayName() ?? "Default Target";
    }

    public void SetDefaultTargetToNull() {
        Ranger.DefaultTarget = null;
        rangerDefaultTargetButton.ButtonParagraph.Text = "Default Target";
    }

    private void ScaleText(object sender, EventArgs e) {
        rangerHireLabel.Scale = SettingsMenu.Scale;
        rangerHireMinus.ButtonParagraph.Scale = SettingsMenu.Scale;
        rangerHirePlus.ButtonParagraph.Scale = SettingsMenu.Scale;
        jeepRentFeeDisplayLabel.Scale = SettingsMenu.Scale;
        jeepRentFeeLabel.Scale = SettingsMenu.Scale;
    }

    private void RangerMinusBtn(GeonBit.UI.Entities.Entity entity) {
        if (GameScene.Active.Model.RangerCount == 0) {
            new AlertMenu("Ranger count", "You don't have any more rangers to fire").Show();
            return;
        }
        foreach (Model.Entities.Entity ent in Ranger.ActiveEntities) {
            if (ent is Ranger ranger) {
                (ranger).Fire();
                break;
            }
        }
        update = true;
    }

    private void RangerPlusBtn(GeonBit.UI.Entities.Entity entity) {
        Ranger temp = new Ranger(MapBuilder.GetRandomSpawn(GameScene.Active.Model.Level));
        rangerTargetIndex = (rangerTargetIndex + 1) % Enum.GetValues(typeof(AnimalSpecies)).Length;
        Game.AddObject(temp);
        rangerHireLabel.Text = GameScene.Active.Model.RangerCount.ToString();
        update = true;
    }

    private void UpdateCurrentTab() {
        if (currentPanel == EntityManagerTab.RangerTab) {
            rangerHireLabel.Text = GameScene.Active.Model.RangerCount.ToString();
            UpdateRangerList();
        } else if (currentPanel == EntityManagerTab.AnimalTab) {
            UpdateAnimalList();
        } else if (currentPanel == EntityManagerTab.OtherTab) {
            UpdateOtherList();
        }
    }

    private void ListAnimal(List<Animal> animals) {
        if (animals.Count < 1) {
            return;
        }
        int count = 0;
        AnimalSpecies currenSpecies = animals[0].Species;
        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);

        StyleProperty deleteBase = new StyleProperty(Color.Red);
        StyleProperty deleteHover = new StyleProperty(Color.DarkRed);
        StyleProperty deleteClick = new StyleProperty(Color.IndianRed);

        StyleProperty chipBase = new StyleProperty(Color.Green);
        StyleProperty chipHover = new StyleProperty(Color.DarkGreen);
        StyleProperty chipClick = new StyleProperty(Color.DarkOliveGreen);
        StyleProperty chipDisabled = new StyleProperty(Color.Gray);

        Label tempAnimalLabel = new Label("", Anchor.AutoCenter, new Vector2(0, 0.25f));
        tempAnimalLabel.Scale = SettingsMenu.Scale;
        tempAnimalLabel.Text = currenSpecies.GetDisplayName() + " (" + animals.Count + ")";
        animalListPanel.AddChild(tempAnimalLabel);
        foreach (var item in animals) {
            Panel tempPanel = new Panel(new Vector2(0, 0.25f), PanelSkin.None, Anchor.Auto);
            Panel tempControllerPanel = new Panel(new Vector2(0.4f, 0), PanelSkin.Simple, Anchor.CenterLeft);

            tempPanel.Padding = new Vector2(0);
            tempPanel.MaxSize = new Vector2(0, 80);
            tempControllerPanel.Padding = new Vector2(0);
            tempControllerPanel.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                if (GameScene.Active.Model.IsDaytime || item.HasChip) {
                    new AnimalControllerMenu(item).Show();
                    Camera.Active.GetComponent<CameraControllerCmp>().CenterOnPosition(item.CenterPosition);
                    Toggle();
                }
            };

            Label tempLabel = new Label(currenSpecies.GetDisplayName() + "#" + (++count).ToString(), Anchor.CenterLeft, new Vector2(0), new Vector2(15, 0));
            tempLabel.ClickThrough = true;
            tempLabel.Scale = SettingsMenu.Scale;

            Button tempButtonSell = new Button("Sell", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.3f, 0));
            Button tempButtonChip = new Button("Chip\n(250)", ButtonSkin.Default, Anchor.AutoInline, new Vector2(0.3f, 0));

            tempButtonSell.Padding = new Vector2(0);
            tempButtonSell.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                item.Sell();
                this.update = true;
            };
            tempButtonSell.SetStyleProperty("FillColor", deleteBase, EntityState.Default);
            tempButtonSell.SetStyleProperty("FillColor", deleteHover, EntityState.MouseHover);
            tempButtonSell.SetStyleProperty("FillColor", deleteClick, EntityState.MouseDown);

            if (item.HasChip) {
                tempButtonChip.ButtonParagraph.Text = "Chipped";
                tempButtonChip.Locked = true;
                tempButtonChip.SetStyleProperty("FillColor", chipDisabled, EntityState.Default);
                tempButtonChip.SetStyleProperty("FillColor", chipDisabled, EntityState.MouseHover);
                tempButtonChip.SetStyleProperty("FillColor", chipDisabled, EntityState.MouseDown);
            } else {
                tempButtonChip.SetStyleProperty("FillColor", chipBase, EntityState.Default);
                tempButtonChip.SetStyleProperty("FillColor", chipHover, EntityState.MouseHover);
                tempButtonChip.SetStyleProperty("FillColor", chipClick, EntityState.MouseDown);
            }
            tempButtonChip.Padding = new Vector2(0);
            tempButtonChip.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                if (GameScene.Active.Model.Funds <= 250) {
                    var alert = new AlertMenu("Can't buy chip", "You can't afford this.");
                    alert.Show();
                    return;
                }
                GameScene.Active.Model.Funds -= 250;
                item.HasChip = true;
                if (entity is Button button) {
                    button.ButtonParagraph.Text = "Chipped";
                }
                entity.Locked = true;

                entity.SetStyleProperty("FillColor", chipDisabled, EntityState.Default);
                entity.SetStyleProperty("FillColor", chipDisabled, EntityState.MouseHover);
                entity.SetStyleProperty("FillColor", chipDisabled, EntityState.MouseDown);
            };


            tempControllerPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
            tempControllerPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);

            tempControllerPanel.AddChild(tempLabel);
            tempPanel.AddChild(tempControllerPanel);
            tempPanel.AddChild(tempButtonChip);
            tempPanel.AddChild(tempButtonSell);
            animalListPanel.AddChild(tempPanel);
        }
    }

    private void UpdateAnimalList() {
        animalListPanel.ClearChildren();

        List<Model.Entities.Entity> tempList = Animal.ActiveEntities.Where(x => x is Animal).OrderBy(x => ((Animal)x).DisplayName).ToList();

        List<Animal> listZebra = new List<Animal>();
        List<Animal> listElephant = new List<Animal>();
        List<Animal> listGiraffe = new List<Animal>();
        List<Animal> listLion = new List<Animal>();
        List<Animal> listTiger = new List<Animal>();
        List<Animal> listTigerWhite = new List<Animal>();

        foreach (var item in Animal.ActiveEntities) {
            if (item is Zebra zebra) {
                listZebra.Add(zebra);
            } else if (item is Elephant elephant) {
                listElephant.Add(elephant);
            } else if (item is Giraffe giraffe) {
                listGiraffe.Add(giraffe);
            } else if (item is Lion lion) {
                listLion.Add(lion);
            } else if (item is Tiger tiger) {
                listTiger.Add(tiger);
            } else if (item is TigerWhite tigerWhite) {
                listTigerWhite.Add(tigerWhite);
            }
        }

        ListAnimal(listElephant);
        ListAnimal(listGiraffe);
        ListAnimal(listZebra);
        ListAnimal(listLion);
        ListAnimal(listTiger);
        ListAnimal(listTigerWhite);
    }

    private void UpdateOtherList() {
        //throw new NotImplementedException();
    }

    private void UpdateRangerList() {
        int count = 0;
        rangerListPanel.ClearChildren();
        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);

        StyleProperty deleteBase = new StyleProperty(Color.Red);
        StyleProperty deleteHover = new StyleProperty(Color.DarkRed);
        StyleProperty deleteClick = new StyleProperty(Color.IndianRed);

        foreach (Model.Entities.Entity ent in Ranger.ActiveEntities) {
            if (ent is Ranger ranger) {
                //DebugConsole.Instance.Write("ranger#" + (count + 1) + (ranger.ChaseTarget == null ? "null" : ranger.ChaseTarget.DisplayName) + " " + (ranger.TargetSpecies == null ? "Default" : ranger.TargetSpecies.ToString()));
                Panel tempPanel = new Panel(new Vector2(0, 0.25f), PanelSkin.None, Anchor.Auto);
                Panel tempControllerPanel = new Panel(new Vector2(0.7f, 0), PanelSkin.Simple, Anchor.CenterLeft);

                tempPanel.Padding = new Vector2(0);
                tempControllerPanel.Padding = new Vector2(0);
                tempControllerPanel.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                    new RangerControllerMenu(ranger).Show();
                    Camera.Active.GetComponent<CameraControllerCmp>().CenterOnPosition(ranger.CenterPosition);
                    Toggle();
                };

                Label tempLabel = new Label("Ranger#" + (++count).ToString(), Anchor.CenterLeft, new Vector2(0.5f, 0), new Vector2(15, 0));
                tempLabel.ClickThrough = true;
                tempLabel.Scale = SettingsMenu.Scale;

                Button tempButtonFire = new Button("Fire", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.3f, 0));
                tempButtonFire.Padding = new Vector2(0);
                tempButtonFire.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                    ranger.Fire();
                    this.update = true;
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
        //DebugConsole.Instance.Write("--------");
    }

    public override void Update(GameTime gameTime) {
        if (!visible) {
            return;
        }
        if (update) {
            update = false;
            UpdateCurrentTab();
        } else if (prevRangerCount != GameScene.Active.Model.RangerCount) {
            UpdateCurrentTab();
        }
        prevRangerCount = GameScene.Active.Model.RangerCount;
    }
}
