using Microsoft.Xna.Framework;
using Safari.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafariTest.Tests.Model;

[TestClass]	
public class LightManagerTest {
	
	[TestMethod("Light manager init, basic small lights & bound checks")]
	public void LightManagerBasics() {
		// Initializes a 2x2 LM with no lights
		LightManager lm = new LightManager(2, 2, 32);
		Assert.IsFalse(lm.CheckLight(new Point(0, 0)));
		Assert.IsFalse(lm.CheckLight(new Point(1, 0)));
		Assert.IsFalse(lm.CheckLight(0, 1));
		Assert.IsFalse(lm.CheckLight(1, 1));

		bool exceptionThrown = false;
		try {
			lm.CheckLight(-1, 0);
			lm.CheckLight(2, 0);
		} catch {
			exceptionThrown = true;
		}
		Assert.IsTrue(exceptionThrown);

		lm.AddLightSource(new Point(0, 0), 0);
		Assert.IsTrue(lm.CheckLight(0, 0));
		exceptionThrown = false;
		try {
			lm.AddLightSource(2, -1, 0);
		} catch {
			exceptionThrown = true;
		}
		Assert.IsTrue(exceptionThrown);

		lm.RemoveLightSource(new Point(0, 0), 0);
		Assert.IsFalse(lm.CheckLight(0, 0));
		exceptionThrown = false;
		try {
			lm.RemoveLightSource(2, -1, 0);
		} catch {
			exceptionThrown = true;
		}
		Assert.IsTrue(exceptionThrown);
	}

	[TestMethod("Light manager overlapping light sources")]
	public void LightManagerOverlaps() {
		// Init 4 x 4 lm
		LightManager lm = new LightManager(4, 4, 32);
		// add a source with range 1 at 0 0 (lights up (0,0), (1, 0) and (0, 1))
		lm.AddLightSource(0, 0, 1);
		Assert.IsTrue(lm.CheckLight(0, 0));
		Assert.IsTrue(lm.CheckLight(1, 0));
		Assert.IsTrue(lm.CheckLight(0, 1));
		Assert.IsFalse(lm.CheckLight(1, 1));
		// add a source with range 1 at 1 1 (lights up (1, 0), (0, 1), (1, 1), (2, 1) and (1, 2))
		lm.AddLightSource(1, 1, 1);
		Assert.IsTrue(lm.CheckLight(1, 1));
		Assert.IsTrue(lm.CheckLight(2, 1));
		Assert.IsTrue(lm.CheckLight(1, 2));
		// remove the first light source ((0, 0) should go dark, but (1,0) and (0,1) should stay lit)
		lm.RemoveLightSource(0, 0, 1);
		Assert.IsFalse(lm.CheckLight(0, 0));
		Assert.IsTrue(lm.CheckLight(1, 0));
		Assert.IsTrue(lm.CheckLight(0, 1));
	}
}
