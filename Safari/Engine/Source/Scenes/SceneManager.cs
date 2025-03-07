namespace Engine.Scenes;

struct ScheduledScene {
	public Scene scene;
	public bool load, unload;

	public ScheduledScene(Scene scene, bool load, bool unload) {
		this.scene = scene;
		this.load = load;
		this.unload = unload;
	}
}

public static class SceneManager {
	public static Scene Active { get; private set; }

	public static bool HasLoadingScheduled => scheduledScene != null;
	private static ScheduledScene? scheduledScene = null;

	public static void Load(Scene scene, bool load = true, bool unload = true) {
		scheduledScene = new ScheduledScene(scene, load, unload);

		if (Active == null) {
			PerformScheduledLoad();
		}
	}

	public static void PerformScheduledLoad() {
		if (Active != null && scheduledScene.Value.unload && Active.Loaded) {
			Active.Unload();
		}

		Active = scheduledScene.Value.scene;
		if (scheduledScene.Value.load && !scheduledScene.Value.scene.Loaded) {
			Active.Load();
		}

		scheduledScene = null;
	}
}
