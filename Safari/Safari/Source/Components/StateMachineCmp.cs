using Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Safari.Components;

public delegate void BeginStateHandler();
public delegate void EndStateHandler();
public delegate void UpdateStateHandler(GameTime gameTime);

/// <summary>
/// Abstract attribute type for state handler method attributes
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class StateHandlerAttribute : Attribute {
	public object TargetState { get; private init; }

	public StateHandlerAttribute(object targetState) {
		TargetState = targetState;
	}
}
/// <summary>
/// Marks a method to run when a given state is entered by the object's state machine
/// </summary>
/// <param name="targetState">The state which triggers the handler at its start</param>
public sealed class StateBeginAttribute(object targetState) : StateHandlerAttribute(targetState) { }
/// <summary>
/// Marks a method to run when a given state is left by the object's state machine
/// </summary>
/// <param name="targetState">The state which triggers the handler at its end</param>
public sealed class StateEndAttribute(object targetState) : StateHandlerAttribute(targetState) { }
/// <summary>
/// Marks a method to run every frame when a given state is active
/// </summary>
/// <param name="targetState">The state which triggers the handler during its updates</param>
public sealed class StateUpdateAttribute(object targetState) : StateHandlerAttribute(targetState) { }

/// <summary>
/// Component for storing a state machine based on an enum type
/// </summary>
/// <typeparam name="T">The enum type to use as a states list</typeparam>
public class StateMachineCmp<T> : Component, IUpdatable where T : Enum {
	/// <summary>
	/// The current state of the state machine
	/// </summary>
	public T CurrentState { get; private set; }
	/// <summary>
	/// The underlying (enum) type of the state machine's states
	/// </summary>
	public Type BaseType => typeof(T);

	private readonly Dictionary<T, BeginStateHandler> beginStateHandlers = [];
	private readonly Dictionary<T, EndStateHandler> endStateHandlers = [];
	private readonly Dictionary<T, UpdateStateHandler> updateStateHandlers = [];
	private readonly Array enumValues;
	
	/// <param name="startState">The state to start the state machine in. Note that the start state's begin handlers will only be invoked when the component gets loaded.</param>
	public StateMachineCmp(T startState) {
		CurrentState = startState;

		enumValues = Enum.GetValues(typeof(T));
		foreach (T val in enumValues) {
			beginStateHandlers[val] = null;
			updateStateHandlers[val] = null;
			endStateHandlers[val] = null;
		}
	}

	// NOTE: the state machine will invoke the begin handlers of the state that is currently in use when the component gets loaded
	public override void Load() {
		foreach (MethodInfo methodInfo in Owner.GetType().GetMethods()) {
			StateBeginAttribute beginAttr = (StateBeginAttribute)methodInfo.GetCustomAttribute(typeof(StateBeginAttribute));
			StateEndAttribute endAttr = (StateEndAttribute)methodInfo.GetCustomAttribute(typeof(StateEndAttribute));
			StateUpdateAttribute updateAttr = (StateUpdateAttribute)methodInfo.GetCustomAttribute(typeof(StateUpdateAttribute));

			if (beginAttr != null) {
				beginStateHandlers[(T)beginAttr.TargetState] += () => methodInfo?.Invoke(Owner, []);
			} else if (endAttr != null) {
				endStateHandlers[(T)endAttr.TargetState] += () => methodInfo?.Invoke(Owner, []);
			} else if (updateAttr != null) {
				updateStateHandlers[(T)updateAttr.TargetState] += (GameTime gameTime) => methodInfo?.Invoke(Owner, [gameTime]);
			}
		}

		beginStateHandlers[CurrentState]?.Invoke();

		base.Load();
	}

	// NOTE: the state machine will invoke the end handlers of the state that is currently in use when the component gets unloaded
	public override void Unload() {
		endStateHandlers[CurrentState]?.Invoke();

		foreach (T val in enumValues) {
			beginStateHandlers[val] = null;
			endStateHandlers[val] = null;
			updateStateHandlers[val] = null;
		}

		base.Unload();
	}

	/// <summary>
	/// Changes the current state
	/// </summary>
	/// <param name="state">The state to transition into</param>
	/// <param name="reenter">Whether to allow same-state reentry (loops in state diagrams/graphs)</param>
	public void Transition(T state, bool reenter = true) {
		if (state.Equals(CurrentState) && !reenter) return;

		endStateHandlers[CurrentState]?.Invoke();

		CurrentState = state;

		beginStateHandlers[CurrentState]?.Invoke();
	}

	public void Update(GameTime gameTime) {
		updateStateHandlers[CurrentState]?.Invoke(gameTime);
	}
}
