using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;

namespace Safari.Scenes.Menus;
public abstract class MenuScene : Scene {
    protected Panel? panel;

    public override void Load() {
        base.Load();
        ConstructUI();
        UserInterface.Active.AddEntity(panel);
    }

    public override void Unload() {
        base.Unload();
        if (panel != null) {
            UserInterface.Active.RemoveEntity(panel);
        }
        DestroyUI();
    }

    protected abstract void ConstructUI();

    protected abstract void DestroyUI();
}
