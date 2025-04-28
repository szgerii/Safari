using Engine.Components;
using GeonBit.UI;
using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Model.Entities.Animals;
using Safari.Scenes;
using Safari.Scenes.Menus;
using System;
using System.Collections.Generic;

namespace Safari.Popups;

public class AnimalMenu : CategoryMenu {
    //private Image zebraImage;
    //private Image giraffeImage;
    //private Image elephantImage;
    //private Image lionImage;
    //private Image tigerImage;
    //private Image tigerWhiteImage;
    private Panel itemsPanel;

    private Panel genderPanel;
    private Button maleButton;
    private Button femaleButton;

    public Gender SelectedGender { get; private set; } = Gender.Female;

    public AnimalMenu() : base("Animal") {
        panel.Size = new Vector2(0.6f, 0.25f);

        itemsPanel = new Panel(new Vector2(0, 0.6f), PanelSkin.None, Anchor.BottomLeft);
        itemsPanel.Padding = new Vector2(0);
        panel.AddChild(itemsPanel);

        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);

        genderPanel = new Panel(new Vector2(0.2f, 0.1f), PanelSkin.Simple, Anchor.BottomRight);
        genderPanel.Padding = new Vector2(10);
        genderPanel.Tag = "PassiveFocus";

        maleButton = new Button("Male", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.5f, 0));
        maleButton.Padding = new Vector2(0);
        maleButton.ToggleMode = true;
        maleButton.OnClick = (Entity entity) => {
            femaleButton.Checked = false;
            maleButton.Checked = true;
            SelectedGender = Gender.Male;
        };
        genderPanel.AddChild(maleButton);

        femaleButton = new Button("Female", ButtonSkin.Default, Anchor.CenterRight, new Vector2(0.5f, 0));
        femaleButton.Padding = new Vector2(0);
        femaleButton.ToggleMode = true;
        femaleButton.OnClick = (Entity entity) => {
            maleButton.Checked = false;
            femaleButton.Checked = true;
            SelectedGender = Gender.Female;
        };
        genderPanel.AddChild(femaleButton);

        femaleButton.Checked = true;

        foreach (AnimalSpecies item in Enum.GetValues<AnimalSpecies>()) {
            Panel tempPanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
            tempPanel.Padding = new Vector2(0,15);
            tempPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
            tempPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
            tempPanel.OnClick = (Entity entity) => {
                Shop.Instance.BuyAnimal(item, SelectedGender);
            };

            Label temp = new Label(item.GetDisplayName() + "(" + item.GetPrice() + ")", Anchor.Center, new Vector2(0));
            temp.ClickThrough = true;
            temp.Padding = new Vector2(0);
            temp.Scale = SettingsMenu.Scale;

            tempPanel.AddChild(temp);

            itemsPanel.AddChild(tempPanel);
        }
    }

    public override void Show() {
        UserInterface.Active.AddEntity(genderPanel);
        base.Show();
        genderPanel.Offset = new Vector2(0, panel.Offset.Y);
        GameScene.Active.MaskedAreas.Add(genderPanel.CalcDestRect());
    }

    public override void Hide() {
        if(panel.Parent == null) {
            return;
        }
        UserInterface.Active.RemoveEntity(genderPanel);
        GameScene.Active.MaskedAreas.Remove(genderPanel.CalcDestRect());
        base.Hide();
    }
}
