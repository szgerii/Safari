using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Safari.Popups;
class DebugConsole : PopupMenu {
    private static DebugConsole instance = new DebugConsole();
    private Paragraph consoleLog;
    private VerticalScrollbar scrollbar;
    private TextInput input;
    private bool visible;

    public static DebugConsole Instance => instance;

    private DebugConsole() {
        this.visible = false;
        this.panel = new Panel(new Vector2(0.7f, 0.5f), PanelSkin.Default, Anchor.TopLeft);
        this.panel.Padding = new Vector2(0);
        this.panel.MinSize = new Vector2(700, 500);
    }

    public void ToggleDebugConsole() {
        if (visible) {
            UserInterface.Active.RemoveEntity(this.panel);
            visible = false;
        } else {
            UserInterface.Active.AddEntity(this.panel);
            visible = true;
        }
    }
}
