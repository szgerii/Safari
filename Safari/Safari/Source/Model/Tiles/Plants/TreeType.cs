using Engine.Graphics.Stubs.Texture;
using Engine.Helpers;

namespace Safari.Model.Tiles;

/// <summary>
/// The tree types that a <see cref="Tree"/> tile can become
/// </summary>
public enum TreeType {
	Digitata,
	Grandideri,
	ShortGrandideri,
	Gregorii,
    Rubrostipa,
	Suarazensis,
	Za
}

/// <summary>
/// Class for <see cref="TreeType"/> extension methods
/// </summary>
public static class TreeTypeExtensions {
	/// <summary>
	/// Returns the display name for the given tree type
	/// </summary>
	/// <param name="type">The tree type</param>
	/// <returns>The display name of the type</returns>
	public static string GetDisplayName(this TreeType type) {
		return type switch {
			TreeType.Digitata => "Digitata",
			TreeType.Grandideri => "Grandideri",
			TreeType.ShortGrandideri => "Grandideri (short)",
			TreeType.Gregorii => "Gregorii",
			TreeType.Rubrostipa => "Rubrostipa",
			TreeType.Suarazensis => "Suarazensis",
			TreeType.Za => "Za",
			_ => "UNKNOWN TREE TYPE",
		};
	}

	/// <summary>
	/// Returns the corresponding texture for a given tree type
	/// </summary>
	/// <param name="type">The tree type</param>
	/// <returns>The texture that represents the type</returns>
	public static ITexture2D GetTexture(this TreeType type) {
		return type switch {
			TreeType.Digitata => Game.LoadTexture("Assets/Trees/Digitata"),
			TreeType.Grandideri => Game.LoadTexture("Assets/Trees/Grandideri1"),
			TreeType.ShortGrandideri => Game.LoadTexture("Assets/Trees/Grandideri2"),
			TreeType.Gregorii => Game.LoadTexture("Assets/Trees/Gregorii"),
			TreeType.Rubrostipa => Game.LoadTexture("Assets/Trees/Rubrostipa"),
			TreeType.Suarazensis => Game.LoadTexture("Assets/Trees/Suarazensis"),
			TreeType.Za => Game.LoadTexture("Assets/Trees/Za"),
			_ => null,
		};
	}

	public static Vectangle GetCollider(this TreeType type) {
		return type switch {
			TreeType.Digitata => new(21, 100, 56, 23),
			TreeType.Grandideri => new(28, 120, 36, 25),
			TreeType.ShortGrandideri => new(28, 100, 42, 24),
			TreeType.Gregorii => new(26, 98, 46, 26),
			TreeType.Rubrostipa => new(40, 100, 37, 22),
			TreeType.Suarazensis => new(44, 69, 12, 15),
			TreeType.Za => new(38, 130, 21, 20),
			_ => Vectangle.Empty
		};
	}

	public static float GetYSortOffset(this TreeType type) {
		return type switch {
			TreeType.Digitata => 128,
			TreeType.Grandideri => 144,
			TreeType.ShortGrandideri => 122,
			TreeType.Gregorii => 122,
			TreeType.Rubrostipa => 122,
			TreeType.Suarazensis => 82,
			TreeType.Za => 149,
			_ => type.GetTexture().Height
		};
	}
}
