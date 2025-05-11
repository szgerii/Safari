using Engine;
using Engine.Input;
using GeonBit.UI;
using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Model.Tiles;
using Safari.Scenes;
using Safari.Scenes.Menus;

namespace Safari.Popups;

public class TileMenu : CategoryMenu, IUpdatable {
    private readonly Panel itemsPanel;

    private readonly Panel extrasPanel;

    private readonly Panel treeTypePanel;
    private readonly Panel treeTypeDisplayPanel;
    private readonly Label treeTypeLabel;
    private readonly Button treeTypePlus;
    private readonly Button treeTypeMinus;

    private readonly Button destroyButton;

    private Rectangle extrasMaskArea;

    public TileMenu() : base("Tiles and plants") {
        //grass
        //water
        //road
        //bush
        //tree

        itemsPanel = new Panel(new Vector2(0, 0.6f), PanelSkin.None, Anchor.BottomLeft);
        itemsPanel.Padding = new Vector2(0);
        panel!.AddChild(itemsPanel);

        extrasPanel = new Panel(new Vector2(0.5f, 0.1f), PanelSkin.Simple, Anchor.BottomRight);
        extrasPanel.Padding = new Vector2(0);
        extrasPanel.Tag = "PassiveFocus";

        treeTypePanel = new Panel(new Vector2(0.6f, 0), PanelSkin.Simple, Anchor.CenterRight);
        treeTypePanel.Padding = new Vector2(10);

        treeTypeDisplayPanel = new Panel(new Vector2(0.6f, 0), PanelSkin.None, Anchor.BottomCenter);
        treeTypeDisplayPanel.Padding = new Vector2(0);

        treeTypeLabel = new Label("", Anchor.Center, new Vector2(0));
        treeTypeLabel.Scale = SettingsMenu.Scale;

        treeTypeMinus = new Button("<", ButtonSkin.Default, Anchor.BottomLeft, new Vector2(0.2f, 0));
        treeTypeMinus.Padding = new Vector2(0);
        treeTypeMinus.OnClick = (Entity entity) => {
            if (Shop.CHelper.SelectedIndex != ConstructionHelperCmp.TREE) {
                Shop.CHelper.SelectedIndex = ConstructionHelperCmp.TREE;
            }
            Shop.CHelper.Palette[Shop.CHelper.SelectedIndex].SelectPrev();
            treeTypeLabel.Text = ((TreeType)Shop.CHelper.Palette[Shop.CHelper.SelectedIndex].VariantChoice).GetDisplayName();
        };

        treeTypePlus = new Button(">", ButtonSkin.Default, Anchor.BottomRight, new Vector2(0.2f, 0));
        treeTypePlus.Padding = new Vector2(0);
        treeTypePlus.OnClick = (Entity entity) => {
            if (Shop.CHelper.SelectedIndex != ConstructionHelperCmp.TREE) {
                Shop.CHelper.SelectedIndex = ConstructionHelperCmp.TREE;
            }
            Shop.CHelper.Palette[Shop.CHelper.SelectedIndex].SelectNext();
            treeTypeLabel.Text = ((TreeType)Shop.CHelper.Palette[Shop.CHelper.SelectedIndex].VariantChoice).GetDisplayName();
        };

        treeTypeDisplayPanel.AddChild(treeTypeLabel);
        treeTypePanel.AddChild(treeTypeMinus);
        treeTypePanel.AddChild(treeTypeDisplayPanel);
        treeTypePanel.AddChild(treeTypePlus);

        StyleProperty deleteBase = new StyleProperty(Color.Red);
        StyleProperty deleteHover = new StyleProperty(Color.DarkRed);
        StyleProperty deleteClick = new StyleProperty(Color.IndianRed);

        StyleProperty hover = new StyleProperty(Color.LightGray);
        StyleProperty click = new StyleProperty(Color.LightSlateGray);

        destroyButton = new Button("Destroy", ButtonSkin.Default, Anchor.CenterLeft, new Vector2(0.4f, 0));
        destroyButton.Padding = new Vector2(0);
        destroyButton.ToggleMode = true;
        destroyButton.OnClick = (Entity entity) => {
            if (treeTypePanel.Parent != null) {
                extrasPanel.RemoveChild(treeTypePanel);
            }
            if (GameScene.Active.MouseMode != MouseMode.Demolish) {
                GameScene.Active.MouseMode = MouseMode.Demolish;
                destroyButton.Checked = true;
            } else {
                GameScene.Active.MouseMode = MouseMode.Build;
                destroyButton.Checked = false;
            }
        };

        extrasPanel.AddChild(destroyButton);

        destroyButton.SetStyleProperty("FillColor", deleteBase, EntityState.Default);
        destroyButton.SetStyleProperty("FillColor", deleteHover, EntityState.MouseHover);
        destroyButton.SetStyleProperty("FillColor", deleteClick, EntityState.MouseDown);

        //tree
        Panel treePanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
        treePanel.Padding = new Vector2(0, 15);
        treePanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        treePanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
        treePanel.OnClick = (Entity entity) => {
            if (treeTypePanel.Parent == null) {
                extrasPanel.AddChild(treeTypePanel);
            }
            destroyButton.Checked = false;
            GameScene.Active.MouseMode = MouseMode.Build;
            Shop.CHelper.SelectedIndex = ConstructionHelperCmp.TREE;
            if (treeTypePanel.Parent == null) {
                UserInterface.Active.AddEntity(treeTypePanel);
            }
            treeTypeLabel.Text = ((TreeType)Shop.CHelper.Palette[Shop.CHelper.SelectedIndex].VariantChoice).GetDisplayName();
        };

        Label treeLabel = new Label("Tree" + "(" + Shop.TREE_COST + ")", Anchor.Center, new Vector2(0));
        treeLabel.ClickThrough = true;
        treeLabel.Padding = new Vector2(0);
        treeLabel.Scale = SettingsMenu.Scale;

        treePanel.AddChild(treeLabel);

        itemsPanel.AddChild(treePanel);

        //water
        Panel waterPanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
        waterPanel.Padding = new Vector2(0, 15);
        waterPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        waterPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
        waterPanel.OnClick = (Entity entity) => {
            if (treeTypePanel.Parent != null) {
                extrasPanel.RemoveChild(treeTypePanel);
            }
            destroyButton.Checked = false;
            GameScene.Active.MouseMode = MouseMode.Build;
            Shop.CHelper.SelectedIndex = ConstructionHelperCmp.WATER;
        };

        Label waterLabel = new Label("Water" + "(" + Shop.WATER_COST + ")", Anchor.Center, new Vector2(0));
        waterLabel.ClickThrough = true;
        waterLabel.Padding = new Vector2(0);
        waterLabel.Scale = SettingsMenu.Scale;

        waterPanel.AddChild(waterLabel);

        itemsPanel.AddChild(waterPanel);

        //grass
        Panel grassPanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
        grassPanel.Padding = new Vector2(0, 15);
        grassPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        grassPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
        grassPanel.OnClick = (Entity entity) => {
            if (treeTypePanel.Parent != null) {
                extrasPanel.RemoveChild(treeTypePanel);
            }
            destroyButton.Checked = false;
            GameScene.Active.MouseMode = MouseMode.Build;
            Shop.CHelper.SelectedIndex = ConstructionHelperCmp.GRASS;
        };

        Label grassLabel = new Label("Grass" + "(" + Shop.GRASS_COST + ")", Anchor.Center, new Vector2(0));
        grassLabel.ClickThrough = true;
        grassLabel.Padding = new Vector2(0);
        grassLabel.Scale = SettingsMenu.Scale;

        grassPanel.AddChild(grassLabel);

        itemsPanel.AddChild(grassPanel);

        //road
        Panel roadPanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
        roadPanel.Padding = new Vector2(0, 15);
        roadPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        roadPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
        roadPanel.OnClick = (Entity entity) => {
            if (treeTypePanel.Parent != null) {
                extrasPanel.RemoveChild(treeTypePanel);
            }
            destroyButton.Checked = false;
            GameScene.Active.MouseMode = MouseMode.Build;
            Shop.CHelper.SelectedIndex = ConstructionHelperCmp.ROAD;
        };

        Label roadLabel = new Label("Road" + "(" + Shop.ROAD_COST + ")", Anchor.Center, new Vector2(0));
        roadLabel.ClickThrough = true;
        roadLabel.Padding = new Vector2(0);
        roadLabel.Scale = SettingsMenu.Scale;

        roadPanel.AddChild(roadLabel);

        itemsPanel.AddChild(roadPanel);

        //bush
        Panel bushPanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
        bushPanel.Padding = new Vector2(0, 15);
        bushPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        bushPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
        bushPanel.OnClick = (Entity entity) => {
            if (treeTypePanel.Parent != null) {
                extrasPanel.RemoveChild(treeTypePanel);
            }
            destroyButton.Checked = false;
            GameScene.Active.MouseMode = MouseMode.Build;
            Shop.CHelper.SelectedIndex = ConstructionHelperCmp.BUSH;
            Shop.CHelper.SelectedItem.VariantChoice = 0;
        };

        Label bushLabel = new Label("Bush" + "(" + Shop.BUSH_COST + ")", Anchor.Center, new Vector2(0));
        bushLabel.ClickThrough = true;
        bushLabel.Padding = new Vector2(0);
        bushLabel.Scale = SettingsMenu.Scale;

        bushPanel.AddChild(bushLabel);

        itemsPanel.AddChild(bushPanel);

        //widewidebush
        Panel widebushPanel = new Panel(new Vector2(0.33f, 0.5f), PanelSkin.Simple, Anchor.AutoInline);
        widebushPanel.Padding = new Vector2(0, 15);
        widebushPanel.SetStyleProperty("FillColor", hover, EntityState.MouseHover);
        widebushPanel.SetStyleProperty("FillColor", click, EntityState.MouseDown);
        widebushPanel.OnClick = (Entity entity) => {
            if (treeTypePanel.Parent != null) {
                extrasPanel.RemoveChild(treeTypePanel);
            }
            GameScene.Active.MouseMode = MouseMode.Build;
            Shop.CHelper.SelectedIndex = ConstructionHelperCmp.BUSH;
            Shop.CHelper.Palette[Shop.CHelper.SelectedIndex].VariantChoice = 1;
        };

        Label widebushLabel = new Label("Wide Bush" + "(" + Shop.BUSH_COST + ")", Anchor.Center, new Vector2(0));
        widebushLabel.ClickThrough = true;
        widebushLabel.Padding = new Vector2(0);
        widebushLabel.Scale = SettingsMenu.Scale;

        widebushPanel.AddChild(widebushLabel);

        itemsPanel.AddChild(widebushPanel);
    }

    private void BuildTile() {
        if (InputManager.Mouse.IsDown(MouseButtons.LeftButton)) {
            Point p = (GameScene.Active.GetMouseTilePos() / GameScene.Active.Model.Level!.TileSize).ToPoint();
            if (GameScene.Active.MouseMode == MouseMode.Build) {
                int price = 0;
                switch (Shop.CHelper.SelectedIndex) {
                    case ConstructionHelperCmp.ROAD:
                        price = Shop.ROAD_COST;
                        break;
                    case ConstructionHelperCmp.GRASS:
                        price = Shop.GRASS_COST;
                        break;
                    case ConstructionHelperCmp.WATER:
                        price = Shop.WATER_COST;
                        break;
                    case ConstructionHelperCmp.BUSH:
                        price = Shop.BUSH_COST;
                        break;
                    case ConstructionHelperCmp.TREE:
                        price = Shop.TREE_COST;
                        break;
                }
                if (GameScene.Active.Model.Funds <= price) {
                    new AlertMenu("Funds", "You don't have enough money for this.").Show();
                    return;
                }
                if (!Shop.CHelper.CanBuild(p, Shop.CHelper.Palette[Shop.CHelper.SelectedIndex].Instance!)) {
                    return;
                }
                Shop.CHelper.BuildCurrent(p);
                GameScene.Active.Model.Funds -= price;
            }
        }
    }

    private void DestroyTile() {
        if (InputManager.Mouse.IsDown(MouseButtons.LeftButton)) {
            Point p = (GameScene.Active.GetMouseTilePos() / GameScene.Active.Model.Level!.TileSize).ToPoint();
            Shop.CHelper.Demolish(p);
        }
    }

    public override void Show() {
        GameScene.Active.MouseMode = MouseMode.Build;
        UserInterface.Active.AddEntity(extrasPanel);
        destroyButton.Checked = false;
        base.Show();
        extrasPanel.Offset = new Vector2(0, panel!.Offset.Y);
        extrasMaskArea = extrasPanel.CalcDestRect();
        GameScene.Active.MaskedAreas.Add(extrasMaskArea);
    }

    public override void Hide() {
        GameScene.Active.MouseMode = MouseMode.Inspect;
        if (extrasPanel.Parent != null) {
            UserInterface.Active.RemoveEntity(extrasPanel);
        }
        GameScene.Active.MaskedAreas.Remove(extrasMaskArea);
        base.Hide();
    }

    public override void Update(GameTime gameTime) {
        Vector2 mouseTilePos = GameScene.Active.GetMouseTilePos();
        if (GameScene.Active.MousePlayable(mouseTilePos)) {
            if (GameScene.Active.MouseMode == MouseMode.Build) {
                BuildTile();
            } else if (GameScene.Active.MouseMode == MouseMode.Inspect) {
                GameScene.Active.UpdateInspect();
            } else if (GameScene.Active.MouseMode == MouseMode.Demolish) {
                DestroyTile();
            }
        }
    }
}
