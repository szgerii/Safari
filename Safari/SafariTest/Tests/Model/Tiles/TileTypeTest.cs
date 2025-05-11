using Microsoft.Xna.Framework;
using Safari.Model.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafariTest.Tests.Model.Tiles;

[TestClass]
public class TileTypeTest {
	[TestMethod]
	public void SmallPlants() {
		Grass g = new Grass();
		Assert.AreEqual(Point.Zero, g.AnchorTile);
		Assert.IsTrue(g.IsFoodSource);
		Assert.IsFalse(g.IsWaterSource);
		Assert.AreEqual(1, g.LightRange);

		Bush b = new Bush();
		Assert.AreEqual(Point.Zero, b.AnchorTile);
		Assert.IsTrue(b.IsFoodSource);
		Assert.IsFalse(b.IsWaterSource);
		Assert.AreEqual(1, b.LightRange);

		WideBush wb = new WideBush();
		Assert.AreEqual(Point.Zero, wb.AnchorTile);
		Assert.IsTrue(wb.IsFoodSource);
		Assert.IsFalse(wb.IsWaterSource);
		Assert.AreEqual(2, wb.LightRange);
	}

	[TestMethod]
	public void Trees() {
		TreeType[] type1 = new[] { TreeType.Digitata, TreeType.ShortGrandideri, TreeType.Gregorii, TreeType.Rubrostipa };
		TreeType[] type2 = new[] { TreeType.Grandideri, TreeType.Za };
		TreeType[] type3 = new[] { TreeType.Suarazensis };

		foreach (TreeType type in type1) {
			Tree t = new Tree(type);
			Assert.AreEqual(new Point(1, 3), t.AnchorTile);
			Assert.IsTrue(t.IsFoodSource);
			Assert.IsFalse(t.IsWaterSource);
			Assert.AreEqual(5, t.LightRange);
		}
		foreach (TreeType type in type2) {
			Tree t = new Tree(type);
			Assert.AreEqual(new Point(1, 4), t.AnchorTile);
			Assert.IsTrue(t.IsFoodSource);
			Assert.IsFalse(t.IsWaterSource);
			Assert.AreEqual(5, t.LightRange);
		}
		foreach (TreeType type in type3) {
			Tree t = new Tree(type);
			Assert.AreEqual(new Point(1, 2), t.AnchorTile);
			Assert.IsTrue(t.IsFoodSource);
			Assert.IsFalse(t.IsWaterSource);
			Assert.AreEqual(5, t.LightRange);
		}
	}

	[TestMethod]
	public void MiscTiles() {
		Road r = new Road();
		Assert.AreEqual(Point.Zero, r.AnchorTile);
		Assert.IsFalse(r.IsFoodSource);
		Assert.IsFalse(r.IsWaterSource);
		Assert.AreEqual(0, r.LightRange);

		Fence f = new Fence();
		Assert.AreEqual(Point.Zero, f.AnchorTile);
		Assert.IsFalse(f.IsFoodSource);
		Assert.IsFalse(f.IsWaterSource);
		Assert.AreEqual(-1, f.LightRange);

		Water w = new Water();
		Assert.AreEqual(Point.Zero, w.AnchorTile);
		Assert.IsFalse(w.IsFoodSource);
		Assert.IsTrue(w.IsWaterSource);
		Assert.AreEqual(1, w.LightRange);
	}
} 
								   