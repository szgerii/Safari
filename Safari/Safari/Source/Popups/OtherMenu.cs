using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Model.Entities.Tourists;
using Safari.Scenes;
using Safari.Scenes.Menus;

namespace Safari.Popups;

public class OtherMenu : CategoryMenu {
    private readonly Panel itemsPanel;

    public OtherMenu() : base("Other") {
        itemsPanel = new Panel(new Vector2(0, 0.6f), PanelSkin.None, Anchor.BottomLeft);
        itemsPanel.Padding = new Vector2(0);
        panel!.AddChild(itemsPanel);

        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);

        //jeep
        Panel jeepPanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
        jeepPanel.Padding = new Vector2(0, 15);
        jeepPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        jeepPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
        jeepPanel.OnClick = (Entity entity) => {
            if (GameScene.Active.Model.Funds <= Shop.JEEP_COST) {
                new AlertMenu("Funds", "You don't have enough money for this jeep.").Show();
                return;
            }
            Jeep.SpawnJeep();
            GameScene.Active.Model.Funds -= Shop.JEEP_COST;
        };

        Label jeepLabel = new Label("Jeep" + "(" + Shop.JEEP_COST + ")", Anchor.Center, new Vector2(0));
        jeepLabel.ClickThrough = true;
        jeepLabel.Padding = new Vector2(0);
        jeepLabel.Scale = SettingsMenu.Scale;

        jeepPanel.AddChild(jeepLabel);

        itemsPanel.AddChild(jeepPanel);
    }
}
