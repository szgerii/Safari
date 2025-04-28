using Engine.Helpers;
using NSubstitute;
using Microsoft.Xna.Framework;
using Ent = Safari.Model.Entities.Entity;
using SafariTest.Utils;
using Safari.Scenes;
using Safari.Model;
using Engine.Components;
using Engine.Graphics.Stubs.Texture;

namespace SafariTest.Tests.Model.Entity;

[TestClass]
public class EntityBaseTest : SimulationTest {
	[TestMethod("Properties Test")]
	public void TestEntity() {
		Ent entity = Substitute.For<Ent>(new Vector2(500, 500));
		Safari.Game.AddObject(entity);
		RunOneFrame();

		Assert.IsNotNull(entity.NavCmp);
		Assert.IsTrue(entity.Visible);

		GameScene.Active.Model.GameSpeed = GameSpeed.Fast;

		// wait until night
		GameAssert.TrueInNFrames(() => !GameScene.Active.Model.IsDaytime, 2000);

		GameScene.Active.Model.GameSpeed = GameSpeed.Slow;
		Assert.IsTrue(entity.Visible);
		entity.VisibleAtNight = false;
		Assert.IsFalse(entity.Visible);

		bool diedEventFired = false;
		entity.Died += (object? _, EventArgs _) => diedEventFired = true;

		Assert.IsFalse(entity.IsDead);
		entity.Die();

		Assert.IsTrue(entity.IsDead);
		Assert.IsTrue(diedEventFired);
		Assert.IsTrue(GameScene.Active.GameObjects.Contains(entity));
		RunOneFrame();
		Assert.IsFalse(GameScene.Active.GameObjects.Contains(entity));
	}

	[TestMethod("Bounds Test")]
	public void TestBounds() {
		Ent entity = Substitute.For<Ent>(new Vector2(500, 500));

		void AssertBounds(Vectangle expectedBounds) {
			entity.UpdateBounds();
			Assert.AreEqual(expectedBounds, entity.Bounds);
			Assert.AreEqual(expectedBounds.Center, entity.CenterPosition);
		}

		// override
		entity.Bounds = new Vectangle(10, 10, 20, 20);
		AssertBounds(new Vectangle(510, 510, 20, 20));

		entity.ClearBoundsOverride();
		// sprite
		PrivateObject entityPO = new(entity);
		entityPO.SetProperty("Sprite", new SpriteCmp(new NoopTexture2D(null, 50, 50)));
		entity.Sprite.SourceRectangle = new(10, 10, 20, 20);
		entity.Sprite.Origin = new Vector2(30, 30);
		entity.Sprite.Scale = 2;
		AssertBounds(new Vectangle(470, 470, 40, 40));

		// anim sprite
		entityPO.SetProperty("Sprite", new AnimatedSpriteCmp(new NoopTexture2D(null, 90, 120), 3, 3, 10));
		// anim sprite dont support custom Origin for now
		entity.Sprite.Scale = 2;
		AssertBounds(new Vectangle(500, 500, 60, 80));
	}

	[TestMethod("Spatial Queries Test")]
	public void TestSpatial() {
		Ent entity = Substitute.ForPartsOf<Ent>(new Vector2(500, 500));
		entity.Bounds = new Vectangle(0, 0, 200, 200); // center is (600, 600) used for sight/reach origin
		entity.UpdateBounds();
		entity.SightDistance = 5;
		entity.ReachDistance = 2;

		Assert.AreEqual(new Vectangle(440, 440, 320, 320), entity.SightArea);
		Assert.AreEqual(new Vectangle(536, 536, 128, 128), entity.ReachArea);

		Ent entity2 = Substitute.ForPartsOf<Ent>(new Vector2(entity.ReachArea.Left, entity.ReachArea.Top));
		entity2.Bounds = new Vectangle(0, 0, 10, 10);
		entity2.UpdateBounds();
		Safari.Game.AddObject(entity);
		Safari.Game.AddObject(entity2);
		RunOneFrame();

		Assert.IsTrue(entity.CanSee(entity2));
		Assert.IsTrue(entity.CanReach(entity2));

		entity2.Position = new Vector2(entity.ReachArea.Right - 1, entity.ReachArea.Bottom - 1);
		entity2.UpdateBounds();
		RunOneFrame();

		Assert.IsTrue(entity.CanSee(entity2));
		Assert.IsTrue(entity.CanReach(entity2));

		entity2.Position = new Vector2(2000, 2000);
		entity2.UpdateBounds();
		RunOneFrame();

		Assert.IsFalse(entity.CanSee(entity2));
		Assert.IsFalse(entity.CanReach(entity2));

		Ent entity3 = Substitute.ForPartsOf<Ent>(new Vector2(600, 600));
		entity3.Bounds = new Vectangle(0, 0, 10, 10);
		entity3.UpdateBounds();
		Safari.Game.AddObject(entity3);

		RunOneFrame();
		RunOneFrame();

		List<Ent> seenEntities = entity.GetEntitiesInSight();
		List<Ent> reachedEntities = entity.GetEntitiesInReach();

		CollectionAssert.Contains(seenEntities, entity3);
		CollectionAssert.Contains(reachedEntities, entity3);
		CollectionAssert.DoesNotContain(seenEntities, entity2);
		CollectionAssert.DoesNotContain(reachedEntities, entity2);
	}
}
