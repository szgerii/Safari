using Microsoft.Xna.Framework;

namespace Safari.Model.Tiles;

using BM = AutoTileBitmask;
using BMH = AutoTileBitmaskHelper;

public partial class AutoTile : Tile {
	/// <summary>
	/// Sets up the Layout dictionary to use a pre-defined "default" layout
	/// </summary>
	public void UseDefaultLayout() {
		HasDiagonalTiling = true;
		Layout.Clear();

		// no neighbors
		Layout[0] = new Point(0, 3);

		// Straights
		Layout[BM.Bottom] = new Point(0, 0);
		Layout[BM.Bottom | BM.Top] = new Point(0, 1);
		Layout[BM.Top] = new Point(0, 2);
		Layout[BM.Right] = new Point(1, 3);
		Layout[BM.Right | BM.Left] = new Point(2, 3);
		Layout[BM.Left] = new Point(3, 3);

		// 45 deg turns and intersections
		Layout[BM.Bottom | BM.Right] = new Point(1, 0);
		Layout[BM.Bottom | BM.Left] = new Point(3, 0);
		Layout[BM.Top | BM.Right] = new Point(1, 2);
		Layout[BM.Top | BM.Left] = new Point(3, 2);
		Layout[BM.Bottom | BM.Left | BM.Right] = new Point(2, 0);
		Layout[BM.Top | BM.Left | BM.Right] = new Point(2, 2);
		Layout[BM.Bottom | BM.Top | BM.Right] = new Point(1, 1);
		Layout[BM.Bottom | BM.Top | BM.Left] = new Point(3, 1);
		Layout[BMH.Straights] = new Point(2, 1);

		// The third blob in the template
		Layout[BMH.TopLeftCorner | BM.Bottom | BM.Right] = new Point(4, 0);
		Layout[BMH.BottomRightCorner | BM.Left] = new Point(5, 0);
		Layout[BMH.BottomLeftCorner | BM.Right] = new Point(6, 0);
		Layout[BMH.TopRightCorner | BM.Bottom | BM.Left] = new Point(7, 0);
		Layout[BMH.BottomRightCorner | BM.Top] = new Point(4, 1);
		Layout[BMH.BottomRightCorner | BMH.BottomLeftCorner | BMH.TopRightCorner] = new Point(5, 1);
		Layout[BMH.BottomRightCorner | BMH.BottomLeftCorner | BMH.TopLeftCorner] = new Point(6, 1);
		Layout[BMH.BottomLeftCorner | BM.Top] = new Point(7, 1);
		Layout[BMH.TopRightCorner | BM.Bottom] = new Point(4, 2);
		Layout[BMH.TopLeftCorner | BMH.TopRightCorner | BMH.BottomRightCorner] = new Point(5, 2);
		Layout[BMH.TopLeftCorner | BMH.TopRightCorner | BMH.BottomLeftCorner] = new Point(6, 2);
		Layout[BMH.TopLeftCorner | BM.Bottom] = new Point(7, 2);
		Layout[BMH.BottomLeftCorner | BM.Top | BM.Right] = new Point(4, 3);
		Layout[BMH.TopRightCorner | BM.Left] = new Point(5, 3);
		Layout[BMH.TopLeftCorner | BM.Right] = new Point(6, 3);
		Layout[BMH.BottomRightCorner | BM.Top | BM.Left] = new Point(7, 3);

		// The forth blob in the template (except the empty tile)
		Layout[BMH.BottomRightCorner] = new Point(8, 0);
		Layout[BMH.BottomRightCorner | BMH.BottomLeftCorner | BM.Top] = new Point(9, 0);
		Layout[BMH.BottomRightCorner | BMH.BottomLeftCorner] = new Point(10, 0);
		Layout[BMH.BottomLeftCorner] = new Point(11, 0);
		Layout[BMH.TopRightCorner | BMH.BottomRightCorner] = new Point(8, 1);
		Layout[BMH.BottomLeftCorner | BMH.TopRightCorner] = new Point(9, 1);
		Layout[BMH.BottomLeftCorner | BMH.TopLeftCorner | BM.Right] = new Point(11, 1);
		Layout[BMH.BottomRightCorner | BMH.TopRightCorner | BM.Left] = new Point(8, 2);
		Layout[BMH.All] = new Point(9, 2);
		Layout[BMH.TopLeftCorner | BMH.BottomRightCorner] = new Point(10, 2);
		Layout[BMH.BottomLeftCorner | BMH.TopLeftCorner] = new Point(11, 2);
		Layout[BMH.TopRightCorner] = new Point(8, 3);
		Layout[BMH.TopRightCorner | BMH.TopLeftCorner] = new Point(9, 3);
		Layout[BMH.TopRightCorner | BMH.TopLeftCorner | BM.Bottom] = new Point(10, 3);
		Layout[BMH.TopLeftCorner] = new Point(11, 3);
	}
}
