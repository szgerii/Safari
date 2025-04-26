using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Model.Entities.Animals;
using System;

namespace Safari.Popups;

class DefaultAnimalSelectorPopup : PopupMenu {
    private readonly static DefaultAnimalSelectorPopup instance = new DefaultAnimalSelectorPopup();
    public static DefaultAnimalSelectorPopup Instance => instance;
    public bool Visible { get; private set; }
    public static bool Showing => instance.Visible;

    private DefaultAnimalSelectorPopup() {
        Visible = false;
        panel = new Panel(new Vector2(0.4f, 0.4f), PanelSkin.Default, Anchor.Center);
        panel.Padding = new Vector2(15);
        panel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;

        Header header = new Header("Select Default Ranger Target", Anchor.TopCenter);
        panel.AddChild(header);

        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);
        StyleProperty selected = new StyleProperty(Color.SlateGray);

        Panel tempPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.Simple, Anchor.AutoCenter);
        Label tempLabel = new Label("None", Anchor.Center);

        tempPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        tempPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);

        tempLabel.ClickThrough = true;
        tempPanel.OnClick = (GeonBit.UI.Entities.Entity entity) => {
            EntityManager.Instance.SetDefaultTargetToNull();
            this.Hide();
        };
        tempPanel.AddChild(tempLabel);
        panel.AddChild(tempPanel);

        foreach (AnimalSpecies item in Enum.GetValues<AnimalSpecies>()) {
            tempPanel = new Panel(new Vector2(0, 0.2f), PanelSkin.Simple, Anchor.AutoCenter);
            tempLabel = new Label(item.GetDisplayName(), Anchor.Center);

            tempPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
            tempPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);

            tempLabel.ClickThrough = true;
            tempPanel.OnClick = (GeonBit.UI.Entities.Entity entity) => {
                EntityManager.Instance.SetDefaultTarget(item);
                this.Hide();
            };
            tempPanel.AddChild(tempLabel);
            panel.AddChild(tempPanel);
        }
    }

    public override void Show() {
        Visible = true;
        base.Show();
    }

    public override void Hide() {
        Visible = false;
        base.Hide();
    }
}
