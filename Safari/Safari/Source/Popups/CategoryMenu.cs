using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Safari.Popups;

class CategoryMenu : PopupMenu {
    private bool visible = false;
    private Header header;

    public CategoryMenu(string categoryName) {
        panel = new Panel(new Vector2(0.25f, 0.15f), PanelSkin.Default, Anchor.BottomLeft);
        panel.Offset = new Vector2(0, Statusbar.Height);

        header = new Header(categoryName, Anchor.TopCenter);
        panel.AddChild(header);
    }

    public override void Show() {
        visible = true;
        base.Show();
    }

    public override void Hide() {
        visible = false;
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
