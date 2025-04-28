using Engine.Graphics.Stubs.Texture;
using Microsoft.Xna.Framework;
using NSubstitute;
using Pose;
using Safari.Model.Tiles;

namespace SafariTest.Tests.Model.Tiles;

using ATBM = AutoTileBitmask;
using ATBMH = AutoTileBitmaskHelper;

[TestClass]
public class AutoTileTest {
	[TestMethod("Auto Tile Test")]
	public void TestAutoTile() {
		GameTime gt = new();

		NoopTexture2D tex = new(null, 100, 100);
		AutoTile at = Substitute.ForPartsOf<AutoTile>(tex);

		Assert.AreEqual(tex, (NoopTexture2D)at.Texture);
		Assert.IsTrue(at.NeedsUpdate);

		bool updateTexCalled = false;
		at.When((AutoTile at) => at.UpdateTexture()).Do(_ => updateTexCalled = true);
		at.When((AutoTile at) => at.UpdateTexture()).DoNotCallBase();
		at.Update(gt);
		Assert.IsTrue(updateTexCalled);

		at.UseDefaultLayout();
		Assert.IsTrue(at.HasDiagonalTiling);

		Assert.IsTrue(ATBMH.HasDirection(ATBMH.LeftEdge, ATBM.Left));
		Assert.IsTrue(ATBMH.HasDirection(ATBMH.LeftEdge, ATBM.BottomLeft));
		Assert.IsTrue(ATBMH.HasDirection(ATBMH.LeftEdge, ATBM.TopLeft));
		Assert.IsTrue(ATBMH.HasDirection(ATBMH.RightEdge, ATBM.Right));
		Assert.IsTrue(ATBMH.HasDirection(ATBMH.RightEdge, ATBM.BottomRight));
		Assert.IsTrue(ATBMH.HasDirection(ATBMH.RightEdge, ATBM.TopRight));
		Assert.IsTrue(ATBMH.HasDirection(ATBMH.TopEdge, ATBM.Top));
		Assert.IsTrue(ATBMH.HasDirection(ATBMH.BottomEdge, ATBM.Bottom));
	}
}
