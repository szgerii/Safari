using Newtonsoft.Json;

namespace Safari.Model.Tiles;

[JsonObject(MemberSerialization.OptIn)]
public class Grass : AutoTile {
	[JsonConstructor]
	public Grass() : base(Game.LoadTexture("Assets/Grass/Grass")) {
		IsFoodSource = true;
		LightRange = 1;

		Sprite.LayerDepth = 0.5f;
		UseDefaultLayout();
	}
}
