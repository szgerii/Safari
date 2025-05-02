using Newtonsoft.Json;

namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing a single water cell
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Water : AutoTile {
	[JsonConstructor]
	public Water() : base(Game.LoadTexture("Assets/Water/Water")) {
		IsWaterSource = true;
		LightRange = 1;

		UseDefaultLayout();
	}
}
