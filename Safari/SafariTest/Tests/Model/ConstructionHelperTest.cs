using Engine.Graphics.Stubs.Texture;
using Safari.Components;
using Safari.Model;
using Safari.Model.Tiles;
using Microsoft.Xna.Framework;

namespace SafariTest.Tests.Model;

[TestClass]
public class ConstructionHelperTest : SimulationTest {
	[TestMethod]
	public void PaletteItem() {
		PaletteItem pi = new PaletteItem((i) => i % 2 == 0 ? new Road() : new Fence(), 2);
		Assert.AreEqual(2, pi.VariantCount);
		Assert.AreEqual(0, pi.VariantChoice);

		Assert.IsInstanceOfType<Road>(pi.Instance);
		Road r1 = (Road)pi.Instance;
		pi.PrepareNext();
		Assert.IsInstanceOfType<Road>(pi.Instance);
		Road r2 = (Road)pi.Instance;
		Assert.AreNotEqual<Road>((Road)r1, r2);

		pi.SelectNext();
		Assert.AreEqual(1, pi.VariantChoice);
		Assert.IsInstanceOfType<Fence>(pi.Instance);
		pi.SelectNext();
		Assert.AreEqual(0, pi.VariantChoice);
		Assert.IsInstanceOfType<Road>(pi.Instance);

		pi.SelectPrev();
		Assert.AreEqual(1, pi.VariantChoice);
		Assert.IsInstanceOfType<Fence>(pi.Instance);
		pi.SelectPrev();
		Assert.AreEqual(0, pi.VariantChoice);
		Assert.IsInstanceOfType<Road>(pi.Instance);

		pi.VariantChoice = 1;
		Assert.AreEqual(1, pi.VariantChoice);
		Assert.IsInstanceOfType<Fence>(pi.Instance);
		pi.VariantChoice = 0;
		Assert.AreEqual(0, pi.VariantChoice);
		Assert.IsInstanceOfType<Road>(pi.Instance);

		pi.VariantChoice = 0;
		pi.PrepareNext();
		Assert.IsInstanceOfType<Road>(pi.Instance);
		Road r3 = (Road)pi.Instance;
		pi.VariantChoice = 0;
		Road r4 = (Road)pi.Instance;
		Assert.IsInstanceOfType<Road>(pi.Instance);
		Assert.AreEqual(r3, r4);
	}

	[TestMethod]
	public void CHPalette() {
		ConstructionHelperCmp ch = new ConstructionHelperCmp(10, 10);

		Assert.AreEqual(5, ch.Palette.Length);
		Assert.AreEqual(0, ch.SelectedIndex);

		ch.SelectNext();
		Assert.AreEqual(1, ch.SelectedIndex);
		ch.SelectPrev();
		Assert.AreEqual(0, ch.SelectedIndex);
		ch.SelectedIndex = 4;
		Assert.AreEqual(4, ch.SelectedIndex);
		ch.SelectNext();
		Assert.AreEqual(0, ch.SelectedIndex);
		ch.SelectPrev();
		Assert.AreEqual(4, ch.SelectedIndex);

		ch.SelectedIndex = -1;
		Assert.AreEqual(-1, ch.SelectedIndex);
		ch.SelectNext();
		Assert.AreEqual(-1, ch.SelectedIndex);
		ch.SelectPrev();
		Assert.AreEqual(-1, ch.SelectedIndex);

		ch.SelectedIndex = ConstructionHelperCmp.ROAD;
		Assert.IsInstanceOfType<Road>(ch.SelectedInstance);
		ch.SelectedIndex = ConstructionHelperCmp.GRASS;
		Assert.IsInstanceOfType<Grass>(ch.SelectedInstance);
		ch.SelectedIndex = ConstructionHelperCmp.WATER;
		Assert.IsInstanceOfType<Water>(ch.SelectedInstance);
		ch.SelectedIndex = ConstructionHelperCmp.BUSH;
		Assert.IsInstanceOfType<Bush>(ch.SelectedInstance);
		ch.SelectedItem.SelectNext();
		Assert.IsInstanceOfType<WideBush>(ch.SelectedInstance);
		ch.SelectedIndex = ConstructionHelperCmp.TREE;
		Assert.IsInstanceOfType<Tree>(ch.SelectedInstance);
		ch.SelectedItem.SelectNext();
		ch.SelectedItem.SelectNext();
		Assert.AreEqual<int>(2, (int)((Tree)ch.SelectedInstance).Type);

		ch.SelectedIndex = -1;
		Assert.IsNull(ch.SelectedItem);
		Assert.IsNull(ch.SelectedInstance);
	}

	[TestMethod]
	public void CHBuild() {
		Level.PLAY_AREA_CUTOFF_X = 2;
		Level.PLAY_AREA_CUTOFF_Y = 2;
		NoopTexture2D tex = new NoopTexture2D(Engine.Game.Instance?.GraphicsDevice, 3200, 3200);
		Level l = new Level(32, 10, 10, tex);
		ConstructionHelperCmp ch = new ConstructionHelperCmp(10, 10);
		ch.Owner = l;

		
		// Simple build, can build
		ch.SelectedIndex = ConstructionHelperCmp.ROAD;
		Assert.IsNull(l.GetTile(4, 4));
		Assert.IsTrue(ch.CanBuildCurrent(4, 4));

		ch.BuildCurrent(4, 4);
		Assert.IsNotNull(l.GetTile(4, 4));
		Assert.IsInstanceOfType<Road>(l.GetTile(4, 4));
		Assert.IsFalse(ch.CanBuildCurrent(4, 4));
		Assert.AreEqual(ch.CanBuildCurrent(4, 4), ch.CanBuild(4, 4, new Road()));

		// Simple demolish, can build
		ch.Demolish(4, 4);
		Assert.IsNull(l.GetTile(4, 4));
		Assert.IsTrue(ch.CanBuildCurrent(4, 4));

		// playarea bounds
		Assert.IsFalse(ch.CanBuildCurrent(1, 1));
		Assert.IsFalse(ch.CanBuildCurrent(9, 9));

		// insta build
		ch.InstaBuild(4, 4, new Fence());
		Assert.IsNotNull(l.GetTile(4, 4));
		Assert.IsInstanceOfType<Fence>(l.GetTile(4, 4));
		ch.Demolish(4, 4);

		// large tile (tree is 3x3)
		ch.SelectedIndex = ConstructionHelperCmp.TREE;
		ch.SelectedItem.SelectNext();
		ch.SelectedItem.SelectNext();
		ch.SelectedItem.SelectNext(); // some treetype
		Assert.IsTrue(ch.CanBuildCurrent(3, 4));
		ch.BuildCurrent(3, 4);
		ch.SelectedIndex = ConstructionHelperCmp.ROAD;
		Assert.IsFalse(ch.CanBuildCurrent(4, 4)); // cant build road next to tree
		Assert.IsNull(l.GetTile(5, 4)); // Even though there is no tile there
		Assert.IsTrue(ch.CanBuildCurrent(5, 4)); // Because roads only cover 1x1
		ch.SelectedIndex = ConstructionHelperCmp.TREE;
		Assert.IsFalse(ch.CanBuildCurrent(5, 4)); // But trees cover 3x3
		Assert.IsTrue(ch.CanBuildCurrent(6, 4));
		ch.Demolish(5, 4);

		// Unbreakable ponts (roadnetwork start & end)
		Point start = new(3, 4);
		Point end = new(6, 4);
		l.Network.Start = start;
		l.Network.End = end;
		l.SetTile(l.Network.Start, new Road());
		l.SetTile(l.Network.End, new Road());
		ch.Load();
		ch.SelectedIndex = ConstructionHelperCmp.GRASS;
		Assert.IsFalse(ch.CanBuildCurrent(3, 4));
		ch.Demolish(3, 4);
		Assert.IsFalse(ch.CanBuildCurrent(3, 4));
	}
}
