using Engine;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Safari.Popups;

public abstract class PopupMenu : IUpdatable {
    private Panel background = new Panel(new Vector2(0, 0), PanelSkin.None, Anchor.TopLeft);
    protected Panel panel;
    protected bool mousePosUpdate = false;
    protected static Vector2? mousePosStorage = null;
    public static PopupMenu Active = null;

    /// <summary>
    /// Shows the popup.
    /// </summary>
    public virtual void Show() {
        UserInterface.Active.AddEntity(background);
        UserInterface.Active.AddEntity(panel);
        panel.BringToFront();
    }

    /// <summary>
    /// Hides the popup.
    /// </summary>
    public virtual void Hide() {
        UserInterface.Active.RemoveEntity(background);
        UserInterface.Active.RemoveEntity(panel);
    }

    /// <summary>
    /// Hides the popup and focuses the area under it.
    /// </summary>
    public virtual void HideWithRefocus() {
        mousePosUpdate = true;

        UserInterface.Active.RemoveEntity(background);
        UserInterface.Active.RemoveEntity(panel);

        /*mousePosStorage = UserInterface.Active.MouseInputProvider.MousePosition;
        UserInterface.Active.MouseInputProvider.UpdateMousePosition(new Vector2(0, 0));
        UserInterface.Active.MouseInputProvider.DoClick();*/
    }

    public abstract void Update(GameTime gameTime); 
}
