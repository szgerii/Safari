using Engine;
using Engine.Graphics.Stubs.Texture;
using Engine.Objects;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Safari.Components;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Model.Entities.Tourists;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

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
		typeof(Road), typeof(Water), typeof(Grass),
		typeof(Bush), typeof(WideBush), typeof(Tree),
		typeof(Tiger), typeof(TigerWhite), typeof(Lion),
		typeof(Zebra), typeof(Elephant), typeof(Giraffe),
		typeof(AnimalGroup)
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
		// entity / group
		new ((obj, scene) => {
			scene.AddObject(obj);
		}, (obj, scene) => obj is AnimalGroup || obj is Entity),
	};
	private static MethodInfo deserMI;

	private string dirPath;
	public List<SafariSaveHead> Saves { get; private init; } = new();

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
				if (MoveDown(i)) {
					i--;
				}
				continue;
			}
			try {
				using (StreamReader sr = new StreamReader(FileName(i))) {
					SafariSaveHead head = JsonConvert.DeserializeObject<SafariSaveHead>(sr.ReadToEnd());
					Saves.Add(head);
				}
			} catch (Exception e) {
				
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
		Dictionary<GameObject, int> refIDs = new();
		int nextID = 0;

		foreach (GameObject go in GameScene.Active.GameObjects) {
			refIDs.Add(go, nextID);
			nextID++;
		}

		foreach (GameObject go in GameScene.Active.GameObjects) {
			foreach (Type type in GameObjectTypes) {
				if (go.GetType() == type) {
					string dump = JsonConvert.SerializeObject(go);
					string typeName = type.AssemblyQualifiedName;
					List<SafariSaveRefNode> refs = new();
					foreach (PropertyInfo pi in go.GetType().GetRuntimeProperties()) {
						if (pi.GetCustomAttribute(typeof(GameobjectReferencePropertyAttribute)) != null) {
							if (typeof(GameObject).IsAssignableFrom(pi.PropertyType)) {
								string propName = pi.Name;

								GameObject target = (GameObject)go.GetType().GetProperty(propName).GetValue(go, null);
								int id = refIDs.ContainsKey(target) ? refIDs[target] : -1;

								refs.Add(new(propName, JsonConvert.SerializeObject(new List<int>() { id })));
							} else if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) {
								string propName = pi.Name;

								List<int> ids = new();
								IList raw = (IList)go.GetType().GetProperty(propName).GetValue(go, null);
								foreach (object obj in raw) {
									ids.Add(refIDs.ContainsKey((GameObject)obj) ? refIDs[(GameObject)obj] : -1);
								}

								refs.Add(new(propName, JsonConvert.SerializeObject(ids)));
							} else {
								throw new GameModelPersistenceException($"Gameobject reference type not yet supported: {pi.PropertyType.Name}.");
							}
						}
					}

					nodes.Add(new(typeName, dump, refIDs[go], refs));
					break;
				}
			}
		}

		SafariSaveHead head = new SafariSaveHead(modelStr, metadata, nodes);
		using (StreamWriter sw = new StreamWriter(fileName)) {
			sw.WriteLine(JsonConvert.SerializeObject(head));
		}
	}

	public bool Load(int slot) {
		SafariSaveHead head = Saves[slot];
		GameModel model = JsonConvert.DeserializeObject<GameModel>(head.GameCoreSerialized);
		Jeep.Init(400);
		Tourist.Init();

		GameScene scene = new GameScene(model);

		Dictionary<int, GameObject> refs = new();
		List<(GameObject, MethodInfo, Dictionary<string, List<int>>)> setupMethods = new();

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
				refs.Add(node.GameObjectID, gameobject);
				foreach (MethodInfo mi in gameobject.GetType().GetRuntimeMethods()) {
					if (mi.GetCustomAttribute(typeof(PostPersistenceSetupAttribute)) != null) {
						Dictionary<string, List<int>> refIds = new();
						foreach (SafariSaveRefNode rn in node.Refs) {
							List<int> l = JsonConvert.DeserializeObject<List<int>>(rn.ContentSerialized);
							refIds.Add(rn.Name, l);
						}
						setupMethods.Add((gameobject, mi, refIds));
					}
				}
			}
		}

		foreach ((GameObject gameobject, MethodInfo mi, Dictionary<string, List<int>> refIds) in setupMethods) {
			Dictionary<string, List<GameObject>> refObjs = new();
			foreach (string key in refIds.Keys) {
				List<GameObject> current = new();
				foreach (int i in refIds[key]) {
					current.Add(refs[i]);
				}
				refObjs.Add(key, current);
			}
			mi.Invoke(gameobject, new[] { refObjs });
		}

		model.Level.Background = Game.CanDraw ? Game.LoadTexture("Assets/Background/Background") : new NoopTexture2D(null, 3584, 2048);
		SceneManager.Load(scene);
		scene.AddObject(model.Level);

		return true;
	}



	public static bool IsNameAvailable(string parkName) {
		foreach (string dir in Directory.GetDirectories(SAVE_PATH)) {
			string name = Path.GetFileName(dir);
			if (name == parkName) {
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Fetches all existing saved parks, sorted (newest first)
	/// </summary>
	/// <returns></returns>
	public static List<string> ListExistingParkNames() =>
		Directory
		.GetDirectories(SAVE_PATH)
		.OrderByDescending(s => Directory.GetLastWriteTime(s))
		.Select(s => Path.GetFileName(s))
		.ToList();
	

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

	private bool MoveDown(int deleteSlot) {
		bool somethingHappened = false;
		string first = FileName(deleteSlot);
		if (File.Exists(first)) {
			File.Delete(first);
			somethingHappened = true;
		}
		for (int i = deleteSlot + 1; i < MAX_SLOTS; i++) {
			string current = FileName(i);
			string prev = FileName(i - 1);
			if (File.Exists(current)) {
				File.Move(current, prev);
				somethingHappened = true;
			}
		}
		return somethingHappened;
	}
}

public class GameModelPersistenceException : Exception {
	public GameModelPersistenceException() : base() { }
	public GameModelPersistenceException(string message) : base(message) { }
	public GameModelPersistenceException(string message, Exception innerException) : base(message, innerException) { }
}
