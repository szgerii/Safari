using Engine.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Engine.Debug;

public class DebugMode {
	public static bool Enabled { get; private set; } = false;

	public static List<ExecutedDebugFeature> ExecutedFeatures { get; } = new();
	public static List<LoopedDebugFeature> LoopedFeatures { get; } = new();

	public static void Enable() {
		foreach (LoopedDebugFeature feature in LoopedFeatures) {
			if (feature.Enabled) {
				RegisterHandler(feature.RunStage, feature.Handler);
			}
		}

		Enabled = true;
	}

	public static void Disable() {
		foreach (LoopedDebugFeature feature in LoopedFeatures) {
			if (feature.Enabled) {
				UnregisterHandler(feature.RunStage, feature.Handler);
			}
		}

		Enabled = false;
	}

	public static void AddFeature(ExecutedDebugFeature feature) => ExecutedFeatures.Add(feature);
	public static void AddFeature(LoopedDebugFeature feature) => LoopedFeatures.Add(feature);

	public static void Execute(string name) => Execute(GetExecutedFeature(name));
	private static void Execute(ExecutedDebugFeature feature) {
		if (Enabled) {
			feature.Handler();
		}
	}

	public static void EnableFeature(string name) => EnableFeature(GetLoopedFeature(name));
	private static void EnableFeature(LoopedDebugFeature feature) {
		feature.Enabled = true;

		if (Enabled) {
			RegisterHandler(feature.RunStage, feature.Handler);
		}
	}

	public static void DisableFeature(string name) => DisableFeature(GetLoopedFeature(name));
	private static void DisableFeature(LoopedDebugFeature feature) {
		feature.Enabled = false;

		if (Enabled) {
			UnregisterHandler(feature.RunStage, feature.Handler);
		}
	}

	public static void ToggleFeature(string name) => ToggleFeature(GetLoopedFeature(name));
	private static void ToggleFeature(LoopedDebugFeature feature) {
		if (feature.Enabled) {
			DisableFeature(feature);
		} else {
			EnableFeature(feature);
		}
	}

	private static LoopedDebugFeature GetLoopedFeature(string name) {
		LoopedDebugFeature feature = null;
		foreach (LoopedDebugFeature df in LoopedFeatures) {
			if (df.Name == name) {
				feature = df;
			}
		}

		if (feature == null) {
			throw new InvalidOperationException("Invalid looped debug feature name: " + name);
		}

		return feature;
	}

	private static ExecutedDebugFeature GetExecutedFeature(string name) {
		ExecutedDebugFeature feature = null;
		foreach (ExecutedDebugFeature edf in ExecutedFeatures) {
			if (edf.Name == name) {
				feature = edf;
			}
		}

		if (feature == null) {
			throw new InvalidOperationException("Invalid executed debug feature name: " + name);
		}

		return feature;
	}

	private static void RegisterHandler(GameLoopStage runStage, EventHandler<GameTime> handler) {
		switch (runStage) {
			case GameLoopStage.PRE_UPDATE:
				SceneManager.Active.PreUpdate += handler;
				break;

			case GameLoopStage.POST_UPDATE:
				SceneManager.Active.PostUpdate += handler;
				break;

			case GameLoopStage.PRE_DRAW:
				SceneManager.Active.PreDraw += handler;
				break;

			case GameLoopStage.POST_DRAW:
				SceneManager.Active.PostDraw += handler;
				break;
		}
	}

	private static void UnregisterHandler(GameLoopStage runStage, EventHandler<GameTime> handler) {
		switch (runStage) {
			case GameLoopStage.PRE_UPDATE:
				SceneManager.Active.PreUpdate -= handler;
				break;

			case GameLoopStage.POST_UPDATE:
				SceneManager.Active.PostUpdate -= handler;
				break;

			case GameLoopStage.PRE_DRAW:
				SceneManager.Active.PreDraw -= handler;
				break;

			case GameLoopStage.POST_DRAW:
				SceneManager.Active.PostDraw -= handler;
				break;
		}
	}
}