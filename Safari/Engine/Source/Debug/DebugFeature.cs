using Microsoft.Xna.Framework;
using System;

namespace Engine.Debug;

public enum GameLoopStage : byte {
	PRE_UPDATE,
	POST_UPDATE,
	PRE_DRAW,
	POST_DRAW
}

public abstract class DebugFeature {
	public string Name { get; }

	public DebugFeature(string name) {
		Name = name;
	}
}

public class ExecutedDebugFeature : DebugFeature {
	public Action Handler { get; set; }

	public ExecutedDebugFeature(string name, Action handler) : base(name) {
		Handler = handler;
	}
}

public class LoopedDebugFeature : DebugFeature {
	public bool Enabled { get; set; }
	public EventHandler<GameTime> Handler { get; set; }
	public GameLoopStage RunStage { get; set; }

	public LoopedDebugFeature(string name, EventHandler<GameTime> handler, GameLoopStage runStage) : base(name) {
		Handler = handler;
		RunStage = runStage;
	}
}