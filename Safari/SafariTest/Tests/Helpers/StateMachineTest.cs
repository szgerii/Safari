using Engine;
using Microsoft.Xna.Framework;
using Safari.Components;

namespace SafariTest.Tests.Helpers;

public class DummyObject(Vector2 pos) : GameObject(pos) {
	public int State1EnterCalled { get; private set; }
	public int State1UpdateCalled { get; private set; }
	public int State1EndCalled { get; private set; }
	public int State2EnterCalled { get; private set; }
	public int State2UpdateCalled { get; private set; }
	public int State2EndCalled { get; private set; }


	[StateBegin(DummyEnum.State1)]
	public void State1Enter() => State1EnterCalled++;

	[StateUpdate(DummyEnum.State1)]
	public void State1Update(GameTime _) => State1UpdateCalled++;

	[StateEnd(DummyEnum.State1)]
	public void State1End() => State1EndCalled++;

	[StateBegin(DummyEnum.State2)]
	public void State2Enter() => State2EnterCalled++;

	[StateUpdate(DummyEnum.State2)]
	public void State2Update(GameTime _) => State2UpdateCalled++;

	[StateEnd(DummyEnum.State2)]
	public void State2End() => State2EndCalled++;
}

public enum DummyEnum {
	State1,
	State2
}

[TestClass]
public class StateMachineTest {
	private DummyObject go = null!;
	private StateMachineCmp<DummyEnum> stateMachine = null!;

	[TestInitialize]
	public void InitStateMachine() {
		go = new(Vector2.Zero);
		stateMachine = new(DummyEnum.State1);

		go.Attach(stateMachine);
		go.Load();
	}

	[TestCleanup]
	public void CleanupStateMachine() {
		go.Unload();
	}

	[TestMethod("State Machine Initialization Test")]
	public void TestInit() {
		Assert.IsTrue(go.Loaded);
		Assert.IsTrue(stateMachine.Loaded);
		Assert.AreEqual(go, stateMachine.Owner);

		Assert.AreEqual(DummyEnum.State1, stateMachine.CurrentState);
		Assert.AreEqual(typeof(DummyEnum), stateMachine.BaseType);
	}

	[TestMethod("State Machine Transition/Hook Test")]
	public void TestTransition() {
		// check entering initial state
		Assert.AreEqual(1, go.State1EnterCalled);

		stateMachine.Transition(DummyEnum.State2);
		Assert.AreEqual(DummyEnum.State2, stateMachine.CurrentState);
		Assert.AreEqual(1, go.State1EndCalled);
		Assert.AreEqual(1, go.State2EnterCalled);

		for (int i = 0; i < 5; i++) {
			stateMachine.Update(new GameTime());
			Assert.AreEqual(i + 1, go.State2UpdateCalled);
			Assert.AreEqual(0, go.State1UpdateCalled);
		}

		stateMachine.Transition(DummyEnum.State2, false);
		Assert.AreEqual(DummyEnum.State2, stateMachine.CurrentState);
		Assert.AreEqual(0, go.State2EndCalled);
		Assert.AreEqual(1, go.State2EnterCalled);

		stateMachine.Transition(DummyEnum.State2, true);
		Assert.AreEqual(DummyEnum.State2, stateMachine.CurrentState);
		Assert.AreEqual(1, go.State2EndCalled);
		Assert.AreEqual(2, go.State2EnterCalled);

		stateMachine.Unload();
		Assert.AreEqual(2, go.State2EndCalled);

		// reset state machine for (potential) further tests
		stateMachine.Load();
	}
}
