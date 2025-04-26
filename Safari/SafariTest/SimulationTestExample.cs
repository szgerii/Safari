using Engine.Scenes;
using Safari.Model.Entities;
using Safari.Scenes;
using SafariTest.Utils;

namespace SafariTest;

[TestClass]
public class SimulationTestExample : SimulationTest {
	[TestMethod("Example Test")]
	public void TestShowcase() {
		Assert.IsInstanceOfType(SceneManager.Active, typeof(GameScene));
		Assert.AreEqual(0, GameScene.Active.Model.EntityCount);
		
		// arrange
		int cash = GameScene.Active.Model.Funds;
		Ranger ranger = new Ranger(new Microsoft.Xna.Framework.Vector2(300, 300));
		Assert.AreEqual(RangerState.Wandering, ranger.State); // some things can be checked instantly after initalization (*)
		
		// act
		Safari.Game.AddObject(ranger); // use Safari.Game for Add/RemoveObject
		Game.RunOneFrame();
		
		// assert
		Assert.AreEqual(1, GameScene.Active.Model.EntityCount); // (*) while others only happen one frame later (during the object's actual load)
		Assert.IsTrue(cash > GameScene.Active.Model.Funds);
		

		Poacher poacher = new Poacher(new Microsoft.Xna.Framework.Vector2(450, 300));
		Safari.Game.AddObject(poacher);

		// gives the ranger two frames to notice the poacher
		// NOTE: this is more integration testing than unit testing
		GameAssert.AreEqualInNFrames(RangerState.Chasing, () => ranger.State, 2);
		Assert.AreEqual(poacher, ranger.ChaseTarget);
	}
}
