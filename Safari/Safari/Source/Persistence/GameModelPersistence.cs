using Engine;
using Engine.Graphics.Stubs.Texture;
using Engine.Objects;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Safari.Components;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Tourists;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Safari.Persistence;

public class InitializationRule {
	public Action<GameObject, GameScene> Init { get; set; }
	public Func<GameObject, GameScene, bool> Predicate { get; set; }

	public InitializationRule(Action<GameObject, GameScene> init, Func<GameObject, GameScene, bool> predicate) {
		Init = init;
		Predicate = predicate;
	}
}

/// <summary>
/// Handles saving and loading the game models state to / from a .safsav file
/// </summary>
public class GameModelPersistence {
	public const int MAX_SLOTS = 5;
	public const string SAVE_PATH = "saves";
	public static List<Type> GameObjectTypes = new List<Type>() {
		typeof(Camera),
		typeof(EntitySpawner<Tourist>), typeof(EntitySpawner<Poacher>),
		typeof(Road),
		typeof(Water),
		typeof(Grass),
		typeof(Bush),
		typeof(WideBush),
		typeof(Tree)
	};
	public static List<InitializationRule> Rules = new List<InitializationRule>() {
		// camera initializer
		new((obj, scene) => {
			Camera camera = obj as Camera;
			Level l = scene.Model.Level;
			Rectangle bounds = new Rectangle(0, 0, l.MapWidth *l.TileSize, l.MapHeight * l.TileSize);
			Camera.Active = camera;
			CameraControllerCmp controllerCmp = new(bounds);
			camera.Attach(controllerCmp);
			scene.AddObject(Camera.Active);
		}, (obj, scene) => obj is Camera),
		// tourist spawner initializer
		new((obj, scene) => {
			EntitySpawner<Tourist> spawner = obj as EntitySpawner<Tourist>;
			spawner.EntityCount = () => Tourist.Queue.Count;
			spawner.ExtraCondition = () => GameScene.Active.Model.IsDaytime;
			scene.AddObject(spawner);
		}, (obj, scene) => obj is EntitySpawner<Tourist>),
		// poacher spawner initializer
		new((obj, scene) => {
			EntitySpawner<Poacher> spawner = obj as EntitySpawner<Poacher>;
			spawner.EntityCount = () => GameScene.Active.Model.PoacherCount;
			scene.AddObject(spawner);
		}, (obj, scene) => obj is EntitySpawner<Poacher>),
		// tile initializer
		new ((obj, scene) => {
			Tile t = obj as Tile;
			Point tmappos = (t.Position / scene.Model.Level.TileSize).ToPoint() + t.AnchorTile;
			ConstructionHelperCmp cons = scene.Model.Level.ConstructionHelperCmp;
			if (cons.CanBuild(tmappos, t)) {
				cons.InstaBuild(tmappos, t);
			}
		}, (obj, scene) => obj is Tile),
	};
	private static MethodInfo deserMI;

	private string dirPath;
	private List<SafariSaveHead> saves = new();

	static GameModelPersistence() {
		foreach (MethodInfo mi in typeof(JsonConvert).GetRuntimeMethods()) {
			ParameterInfo[] ps = mi.GetParameters();
			if (mi.Name == "DeserializeObject" && mi.IsStatic && mi.IsGenericMethod && ps.Length == 1) {
				deserMI = mi;
			}
		}
	}

	public GameModelPersistence(string parkName) {
		dirPath = Path.Join(SAVE_PATH, parkName);
		if (!Directory.Exists(dirPath)) {
			Directory.CreateDirectory(dirPath);
		}
		for (int i = 0; i < MAX_SLOTS; i++) {
			string current = FileName(i);
			if (!File.Exists(current)) {
				break;
			}
			try {
				using (StreamReader sr = new StreamReader(FileName(i))) {
					SafariSaveHead head = JsonConvert.DeserializeObject<SafariSaveHead>(sr.ReadToEnd());
					saves.Add(head);
				}
			} catch (Exception e) {
				MoveDown(i);
				i--;
			}
		}
	}

	public void Save(GameModel model) {
		string fileName = FileName(0);
		if (File.Exists(fileName)) {
			MoveUp();
		}

		string modelStr = JsonConvert.SerializeObject(model);

		SaveMetadata metadata = new SaveMetadata(DateTime.Now, model.ParkName, TimeSpan.FromSeconds(model.CurrentTime), model.IngameDate, model.PostWin);

		List<SafariSaveNode> nodes = new List<SafariSaveNode>();

		foreach (GameObject go in GameScene.Active.GameObjects) {
			foreach (Type type in GameObjectTypes) {
				if (go.GetType() == type) {
					string dump = JsonConvert.SerializeObject(go);
					string typeName = type.AssemblyQualifiedName;
					nodes.Add(new(typeName, dump));
				}
			}
		}

		SafariSaveHead head = new SafariSaveHead(modelStr, metadata, nodes);
		using (StreamWriter sw = new StreamWriter(fileName)) {
			sw.WriteLine(JsonConvert.SerializeObject(head));
		}
	}

	public bool Load(int slot) {
		SafariSaveHead head = saves[slot];
		GameModel model = JsonConvert.DeserializeObject<GameModel>(head.GameCoreSerialized);
		Jeep.Init(400);
		Tourist.Init();

		GameScene scene = new GameScene(model);
		foreach (SafariSaveNode node in head.SaveNodes) {
			Type objectType = Type.GetType(node.FullTypeName);
			object obj = deserMI
						.MakeGenericMethod(new[] { objectType })
						.Invoke(null, new[] { node.GameObjectSerialized });
			if (obj is GameObject gameobject) {
				foreach (InitializationRule rule in Rules) {
					if (rule.Predicate(gameobject, scene)) {
						rule.Init(gameobject, scene);
						break;
					}
				}
			}
		}

		model.Level.Background = Game.CanDraw ? Game.LoadTexture("Assets/Background/Background") : new NoopTexture2D(null, 3584, 2048);
		SceneManager.Load(scene);
		scene.AddObject(model.Level);

		return true;
	}

	private string FileName(int slot) => Path.Join(dirPath, $"slot{slot}.safsav");

	private void MoveUp() {
		string last = FileName(MAX_SLOTS - 1);
		if (File.Exists(last)) {
			File.Delete(last);
		}
		for (int i = MAX_SLOTS - 2; i >= 0; i--) {
			string current = FileName(i);
			string next = FileName(i + 1);
			if (File.Exists(current)) {
				File.Move(current, next);
			}
		}
	}

	private void MoveDown(int deleteSlot) {
		string first = FileName(deleteSlot);
		if (File.Exists(first)) {
			File.Delete(first);
		}
		for (int i = deleteSlot + 1; i < MAX_SLOTS; i++) {
			string current = FileName(i);
			string prev = FileName(i - 1);
			if (File.Exists(current)) {
				File.Move(current, prev);
			}
		}
	}
}

public class GameModelPersistenceException : Exception {
	public GameModelPersistenceException() : base() { }
	public GameModelPersistenceException(string message) : base(message) { }
	public GameModelPersistenceException(string message, Exception innerException) : base(message, innerException) { }
}
