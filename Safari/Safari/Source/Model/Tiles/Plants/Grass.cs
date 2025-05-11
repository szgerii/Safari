using Newtonsoft.Json;

namespace Safari.Model.Tiles;

/// <summary>
/// A class representing grass tiles, one of the main food sources for animals
/// </summary>
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
