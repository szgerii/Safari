using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Scenes;
using System.Collections.Generic;

namespace Safari.Popups;

public abstract class CategoryMenu : PopupMenu {
    private bool visible = false;
    private readonly Header header;

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
        visible = true;
        panel.Offset = new Vector2(0, Statusbar.Instance.Size.Height - 45);
        base.Show();
    }

    public override void Hide() {
        visible = false;
        base.Hide();
    }

    /// <summary>
    /// Toggles the category menu's visibility.
    /// </summary>
    public void ToggleCategoryMenu() {
        if (visible) {
            Hide();
        } else {
            Show();
        }   
    }
}
