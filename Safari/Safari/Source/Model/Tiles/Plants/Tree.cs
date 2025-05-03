using Engine.Components;
using Engine.Collision;
using Microsoft.Xna.Framework;

namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing a tree
/// </summary>
public class Tree : Tile {

	private static readonly Point[] TreeOffsets = [
		new(-1, -1), new(-1, 0), new(-1, 1),
		new (0, -1), new(0, 1),
		new (1, -1), new(1, 0), new(1, 1)
	];

	private TreeType type;
	/// <summary>
	/// The type of the tree tile
	/// </summary>
	public TreeType Type {
		get => type;
		set {
			type = value;
			Texture = type.GetTexture();

			Size = new Point {
				X = type != TreeType.Rubrostipa ? 3 : 4,
				Y = (type != TreeType.Grandideri && type != TreeType.Za) ? 4 : 5
			};
		}
	}

	/// <summary>
	/// The display name of the current tree type
	/// </summary>
	public string DisplayName => type.GetDisplayName();

	public Tree(TreeType type) : base(type.GetTexture()) {
		ConstructionBlockOffsets = TreeOffsets;
		IsFoodSource = true;
		Type = type;
		LightRange = 5;
		Sprite.LayerDepth = 0.4f;

		switch (Type) {
			case TreeType.Digitata:
			case TreeType.ShortGrandideri:
			case TreeType.Gregorii:
			case TreeType.Rubrostipa:
				AnchorTile = new Point(1, 3);
				break;
			case TreeType.Grandideri:
			case TreeType.Za:
				AnchorTile = new Point(1, 4);
				break;
			case TreeType.Suarazensis:
				AnchorTile = new Point(1, 2);
				break;
			default:
				break;
		}

		CollisionCmp collCmp = new CollisionCmp(type.GetCollider()) {
			Tags = CollisionTags.World
		};
		Attach(collCmp);
	}

	public override void UpdateYSortOffset() {
		Sprite.YSortOffset = Texture.Height;
	}
}
