using Engine.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Debug;

public class DebugMode {
	public static bool Enabled { get; private set; } = false;

	public static List<ExecutedDebugFeature> ExecutedFeatures { get; } = new();
	public static List<LoopedDebugFeature> LoopedFeatures { get; } = new();

	private readonly static Dictionary<string, bool> flags = [];

	public static void Enable() {
		foreach (LoopedDebugFeature feature in LoopedFeatures) {
			if (feature.Enabled && feature.Handler != null) {
				RegisterHandler(feature.RunStage, feature.Handler);
			}
		}

		Enabled = true;
	}

	public static void Disable() {
		foreach (LoopedDebugFeature feature in LoopedFeatures) {
			if (feature.Enabled && feature.Handler != null) {
				UnregisterHandler(feature.RunStage, feature.Handler);
			}
		}

		Enabled = false;
	}

	public static void AddFeature(ExecutedDebugFeature feature) {
		ExecutedFeatures.RemoveAll(f => f.Name == feature.Name);
		ExecutedFeatures.Add(feature);
	}
	public static void AddFeature(LoopedDebugFeature feature) {
		LoopedFeatures.RemoveAll(f => f.Name == feature.Name);
		LoopedFeatures.Add(feature);
	}

	public static bool HasFeature(string name) => HasExecutedFeature(name) || HasLoopedFeature(name);
	public static bool HasExecutedFeature(string name) => ExecutedFeatures.Any(e => e.Name == name);
	public static bool HasLoopedFeature(string name) => LoopedFeatures.Any(e => e.Name == name);

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

	public static bool IsLoopedFeatureEnabled(string name) => GetLoopedFeature(name).Enabled;

	public static void SetFlag(string name, bool value) => flags[name] = value;
	public static bool IsFlagActive(string name) => flags.TryGetValue(name, out bool result) && result;
	public static bool HasFlagBeenSet(string name) => flags.ContainsKey(name);

	public static void EnableFlag(string name) => SetFlag(name, true);
	public static void DisableFlag(string name) => SetFlag(name, false);
	public static void ToggleFlag(string name) => SetFlag(name, !IsFlagActive(name));

	private static LoopedDebugFeature GetLoopedFeature(string name) {
		LoopedDebugFeature? feature = null;
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
		ExecutedDebugFeature? feature = null;
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
				SceneManager.Active!.PreUpdate += handler;
				break;

			case GameLoopStage.POST_UPDATE:
				SceneManager.Active!.PostUpdate += handler;
				break;

			case GameLoopStage.PRE_DRAW:
				SceneManager.Active!.PreDraw += handler;
				break;

			case GameLoopStage.POST_DRAW:
				SceneManager.Active!.PostDraw += handler;
				break;
		}
	}

	private static void UnregisterHandler(GameLoopStage runStage, EventHandler<GameTime> handler) {
		switch (runStage) {
			case GameLoopStage.PRE_UPDATE:
				SceneManager.Active!.PreUpdate -= handler;
				break;

			case GameLoopStage.POST_UPDATE:
				SceneManager.Active!.PostUpdate -= handler;
				break;

			case GameLoopStage.PRE_DRAW:
				SceneManager.Active!.PreDraw -= handler;
				break;

			case GameLoopStage.POST_DRAW:
				SceneManager.Active!.PostDraw -= handler;
				break;
		}
	}
}