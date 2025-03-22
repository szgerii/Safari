using Engine.Scenes;
using GeonBit.UI;
using GeonBit.UI.Entities;

namespace Safari.Scenes.Menus;
public abstract class MenuScene : Scene {
    protected Panel panel;

    public override void Load() {
        base.Load();
        this.ConstructUI();
        UserInterface.Active.AddEntity(panel);
    }

    public override void Unload() {
        base.Unload();
        UserInterface.Active.RemoveEntity(panel);
        this.DestroyUI();
    }

    protected abstract void ConstructUI();

    protected abstract void DestroyUI();
}
