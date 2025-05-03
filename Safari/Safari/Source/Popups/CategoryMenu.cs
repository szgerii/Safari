using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Scenes;
using System.Collections.Generic;

namespace Safari.Popups;

public abstract class CategoryMenu : PopupMenu {
    private bool visible = false;
    private readonly Header header;

    private Rectangle maskArea;

    public bool Visible => visible;

    public CategoryMenu(string categoryName) {
        background = null;

        panel = new Panel(new Vector2(0.5f, 0.25f), PanelSkin.Default, Anchor.BottomLeft);
        panel.Tag = "PassiveFocus";
        panel.Padding = new Vector2(20);

        header = new Header(categoryName, Anchor.TopCenter);
        header.Size = new Vector2(0, 0.2f);
        panel.AddChild(header);
    }

    public override void Show() {
        if (panel.Parent != null) {
            return;
        }
        visible = true;
        panel.Offset = new Vector2(0, Statusbar.Instance.Size.Height - 45);
        maskArea = panel.CalcDestRect();
        GameScene.Active.MaskedAreas.Add(maskArea);
        base.Show();
    }

    public override void Hide() {
        if (panel.Parent == null) {
            return;
        }
        visible = false;
        GameScene.Active.MaskedAreas.Remove(maskArea);
        base.Hide();
    }

    public void ToggleCategoryMenu() {
        if (visible) {
            Hide();
        } else {
            Show();
        }   
    }
}
