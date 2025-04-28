using Microsoft.Xna.Framework;
using NSubstitute;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using SafariTest.Utils;

namespace SafariTest.Tests.Model.Entity;

[TestClass]
public class PoacherTest : SimulationTest
{
    [TestMethod("Properties Test")]
    public void TestProps()
    {
        Poacher poacher = new(new Vector2(500, 500));

        Assert.IsNull(poacher.CaughtAnimal);
        Assert.AreEqual(PoacherState.Wandering, poacher.State);
        Assert.IsFalse(poacher.Visible);
        Assert.AreEqual(typeof(PoacherState), poacher.StateMachine.BaseType);
        Assert.AreEqual("Poacher", poacher.DisplayName);

        poacher.Reveal();
        Assert.IsTrue(poacher.Visible);
    }

    [TestMethod("State Tests")]
    public void TestStates()
    {
        GameTime gt = new();

        Poacher poacher = Substitute.ForPartsOf<Poacher>(new Vector2(500, 500));
        Engine.Game.AddObject(poacher);
        RunOneFrame();

        Vector2 oldPos = poacher.Position;
        GameAssert.TrueInNFrames(() => poacher.NavCmp.Moving, 5);
        Assert.IsNotNull(poacher.NavCmp.TargetPosition);
        RunOneFrame();
        Assert.AreNotEqual(oldPos, poacher.Position);
        Assert.IsTrue(Model.Level.PlayAreaBounds.Contains(poacher.NavCmp.TargetPosition.Value));

        Vector2 oldTarget = poacher.NavCmp.TargetPosition.Value;
        oldPos = poacher.Position;
        // TODO: figure out why these fail occasionally
        //GameAssert.AreNotEqualBefore(oldPos, () => poacher.Position, TimeSpan.FromDays(1));
        //GameAssert.AreNotEqualBefore(oldTarget, () => poacher.NavCmp.TargetPosition, TimeSpan.FromDays(1));
        //Assert.IsTrue(Model.Level.PlayAreaBounds.Contains(poacher.NavCmp.TargetPosition.Value));

        Zebra animal = Substitute.ForPartsOf<Zebra>(new Vector2(poacher.SightArea.Right - 1, poacher.SightArea.Bottom - 1), Gender.Male);
        animal.When((Zebra animal) => animal.Update(Arg.Any<GameTime>())).DoNotCallBase();
        Safari.Game.AddObject(animal);
        RunOneFrame();
        
        poacher.GetEntitiesInSight().Returns([animal]);
        RunOneFrame();

        Assert.AreEqual(PoacherState.Chasing, poacher.State);
        Assert.AreEqual(animal, poacher.ChaseTarget);
        Assert.AreEqual(animal, poacher.NavCmp.TargetObject);

        float oldDist = Vector2.DistanceSquared(poacher.CenterPosition, animal.CenterPosition);
        GameAssert.TrueInNFrames(() => Vector2.DistanceSquared(poacher.CenterPosition, animal.CenterPosition) < oldDist, 5);
        Assert.AreEqual(PoacherState.Chasing, poacher.State);

		Safari.Game.Random = Substitute.For<Random>();
        Safari.Game.Random.NextSingle().Returns(1f);

        Model.GameSpeed = GameSpeed.Fast;
        GameAssert.AreEqualBefore(PoacherState.Smuggling, () => poacher.State, TimeSpan.FromHours(2));
        Model.GameSpeed = GameSpeed.Slow;
        RunOneFrame();

        Assert.IsNull(poacher.ChaseTarget);
        Assert.IsNull(poacher.NavCmp.TargetObject);
        Assert.IsNotNull(poacher.NavCmp.TargetPosition);
        Assert.IsTrue(animal.IsCaught);
        Assert.IsNull(animal.Group);

        // TODO: escape + shoot behavior testing
    }
}
