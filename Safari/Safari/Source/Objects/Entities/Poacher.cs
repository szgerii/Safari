using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Animals;
using Safari.Scenes;

namespace Safari.Objects.Entities;

public class Poacher : Entity {
	private Animal? caughtAnimal = null;
	
	public Poacher(Vector2 pos) : base(pos) {
		DisplayName = "Poacher";
	}

	public override void Load() {
		GameScene.Active.Model.PoacherCount++;

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.PoacherCount--;

		base.Unload();
	}
}
