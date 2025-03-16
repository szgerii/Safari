using System;
using Microsoft.Xna.Framework;

namespace Safari.Model.Tiles;

/// <summary>
/// AutoTile bitmasking values
/// </summary>
[Flags]
public enum AutoTileBitmask {
	TopLeft = 1 << 0,
	Top = 1 << 1,
	TopRight = 1 << 2,
	Left = 1 << 3,
	Right = 1 << 4,
	BottomLeft = 1 << 5,
	Bottom = 1 << 6,
	BottomRight = 1 << 7
}

/// <summary>
/// Helper/extension methods and properties for <see cref="AutoTileBitmask"/>
/// </summary>
public static class AutoTileBitmaskHelper {
	/// <summary>
	/// Bitmask that covers the entire left edge (BL | L | TL)
	/// </summary>
	public static AutoTileBitmask LeftEdge => AutoTileBitmask.Left | AutoTileBitmask.TopLeft | AutoTileBitmask.BottomLeft;
	/// <summary>
	/// Bitmask that covers the entire right edge (BR | R | TR)
	/// </summary>
	public static AutoTileBitmask RightEdge => AutoTileBitmask.Right | AutoTileBitmask.TopRight | AutoTileBitmask.BottomRight;
	/// <summary>
	/// Bitmask that covers the entire top edge (TL | T | TR)
	/// </summary>
	public static AutoTileBitmask TopEdge => AutoTileBitmask.TopLeft | AutoTileBitmask.Top | AutoTileBitmask.TopRight;
	/// <summary>
	/// Bitmask that covers the entire bottom edge (BL | B | BR)
	/// </summary>
	public static AutoTileBitmask BottomEdge => AutoTileBitmask.BottomLeft | AutoTileBitmask.Bottom | AutoTileBitmask.BottomRight;
	/// <summary>
	/// Bitmask that covers the top left corner (L | TL | T)
	/// </summary>
	public static AutoTileBitmask TopLeftCorner => AutoTileBitmask.Left | AutoTileBitmask.TopLeft | AutoTileBitmask.Top;
	/// <summary>
	/// Bitmask that covers the top right corner (R | TR | T)
	/// </summary>
	public static AutoTileBitmask TopRightCorner => AutoTileBitmask.Right | AutoTileBitmask.TopRight | AutoTileBitmask.Top;
	/// <summary>
	/// Bitmask that covers the bottom left corner (L | BL | B)
	/// </summary>
	public static AutoTileBitmask BottomLeftCorner => AutoTileBitmask.Left | AutoTileBitmask.BottomLeft | AutoTileBitmask.Bottom;
	/// <summary>
	/// Bitmask that covers the bottom right corner (R | BR | B)
	/// </summary>
	public static AutoTileBitmask BottomRightCorner => AutoTileBitmask.Right | AutoTileBitmask.BottomRight | AutoTileBitmask.Bottom;
	/// <summary>
	/// Bitmask that combines every possible direction
	/// </summary>
	public static AutoTileBitmask All => (AutoTileBitmask)255;

	/// <summary>
	/// Bitmask that combines every straight direction (L | T | R | B)
	/// </summary>
	public static AutoTileBitmask Straights => AutoTileBitmask.Top | AutoTileBitmask.Left | AutoTileBitmask.Bottom | AutoTileBitmask.Right;
	/// <summary>
	/// Bitmask that combines every diagonal direction (TL | TR | BL | BR)
	/// </summary>
	public static AutoTileBitmask Diagonals => AutoTileBitmask.TopLeft | AutoTileBitmask.TopRight | AutoTileBitmask.BottomLeft | AutoTileBitmask.BottomRight;

	/// <summary>
	/// Checks if a bitmask contains a specific direction
	/// </summary>
	/// <param name="bitmask">The bitmask to check in</param>
	/// <param name="dir">The direction to check</param>
	/// <returns>Whether the bitmask contains the given direction</returns>
	public static bool HasDirection(this AutoTileBitmask bitmask, AutoTileBitmask dir)
		=> (bitmask & dir) == dir;

	/// <summary>
	/// Converts a bitmask to its direction offset
	/// <br/>
	/// Should only be used with singular bitmask values
	/// </summary>
	/// <param name="bitmask">The bitmask to convert</param>
	/// <returns>The direction offset (-1 <= X, Y <= 1)</returns>
	public static Point ToOffset(this AutoTileBitmask bitmask) {
		return bitmask switch {
			0 => Point.Zero,
			AutoTileBitmask.Top => new Point(0, -1),
			AutoTileBitmask.Bottom => new Point(0, 1),
			AutoTileBitmask.Right => new Point(1, 0),
			AutoTileBitmask.Left => new Point(-1, 0),
			AutoTileBitmask.TopLeft => new Point(-1, -1),
			AutoTileBitmask.TopRight => new Point(1, -1),
			AutoTileBitmask.BottomLeft => new Point(-1, 1),
			AutoTileBitmask.BottomRight => new Point(1, 1),
			_ => Point.Zero
		};
	}
}
