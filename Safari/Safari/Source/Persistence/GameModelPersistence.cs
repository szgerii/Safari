using Engine;
using Engine.Debug;
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
	public static string SavePath { get; private set; } = Path.Join(Game.SafariPath, "saves");
	public static List<Type> GameObjectTypes { get; set; } = new List<Type>() {
		typeof(Camera),
		typeof(EntitySpawner<Tourist>), typeof(EntitySpawner<Poacher>),
		typeof(Road), typeof(Water), typeof(Grass),
		typeof(Bush), typeof(WideBush), typeof(Tree),
		typeof(Tiger), typeof(TigerWhite), typeof(Lion),
		typeof(Zebra), typeof(Elephant), typeof(Giraffe),
		typeof(AnimalGroup),
		typeof(Poacher), typeof(Ranger),
		typeof(Jeep), typeof(Tourist)
	};
	public static List<Type> StaticTypes { get; set; } = new List<Type>() {
		typeof(Ranger), typeof(Jeep), typeof(Tourist)
	};
	public static List<InitializationRule> Rules { get; set; } = new List<InitializationRule>() {
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
			Tourist.Spawner = spawner;
			scene.AddObject(spawner);
		}, (obj, scene) => obj is EntitySpawner<Tourist>),
		// poacher spawner initializer
		new((obj, scene) => {
			EntitySpawner<Poacher> spawner = obj as EntitySpawner<Poacher>;
			spawner.EntityCount = () => GameScene.Active.Model.PoacherCount;
			spawner.ExtraCondition = () => !DebugMode.IsFlagActive("no-poachers");
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
		dirPath = Path.Join(SavePath, parkName);
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
			} catch { }
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
		List<SafariSaveStaticNode> staticNodes = new List<SafariSaveStaticNode>();

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
					foreach (PropertyInfo pi in go.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
						if (pi.GetCustomAttribute(typeof(GameobjectReferencePropertyAttribute)) != null) {
							refs.Add(GetRef(pi.Name, pi.PropertyType, refIDs, pi.GetValue(go, null)));
						}
					}
					foreach (FieldInfo fi in go.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
						if (fi.GetCustomAttribute(typeof(GameobjectReferencePropertyAttribute)) != null) {
							refs.Add(GetRef(fi.Name, fi.FieldType, refIDs, fi.GetValue(go)));
						}
					}

					nodes.Add(new(typeName, dump, refIDs[go], refs));
					break;
				}
			}
		}

		foreach (Type type in StaticTypes) {
			string typeName = type.AssemblyQualifiedName;
			List<SafariSaveStaticProp> props = new();
			List<SafariSaveRefNode> refs = new();
			foreach (PropertyInfo pi in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)) {
				if (pi.GetCustomAttribute(typeof(StaticSavedPropertyAttribute)) != null) {
					string propName = pi.Name;
					string propType = pi.PropertyType.AssemblyQualifiedName;
					props.Add(new(propType, propName, true, JsonConvert.SerializeObject(pi.GetValue(null, null))));
				} else if (pi.GetCustomAttribute(typeof(StaticSavedReferenceAttribute)) != null) {
					refs.Add(GetRef(pi.Name, pi.PropertyType, refIDs, pi.GetValue(null, null)));
				}
			}
			foreach (FieldInfo fi in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)) {
				if (fi.GetCustomAttribute(typeof(StaticSavedPropertyAttribute)) != null) {
					string propName = fi.Name;
					string propType = fi.FieldType.AssemblyQualifiedName;
					props.Add(new(propType, propName, false, JsonConvert.SerializeObject(fi.GetValue(null))));
				} else if (fi.GetCustomAttribute(typeof(StaticSavedReferenceAttribute)) != null) {
					refs.Add(GetRef(fi.Name, fi.FieldType, refIDs, fi.GetValue(null)));
				}
			}
			staticNodes.Add(new(typeName, props, refs));
		}

		SafariSaveHead head = new SafariSaveHead(modelStr, metadata, nodes, staticNodes);
		using (StreamWriter sw = new StreamWriter(fileName)) {
			sw.WriteLine(JsonConvert.SerializeObject(head));
		}
	}

	private SafariSaveRefNode GetRef(string name, Type type, Dictionary<GameObject, int> refIDs, object value) {
		if (typeof(GameObject).IsAssignableFrom(type)) {

			GameObject target = (GameObject)value;
			int id = target == null || !refIDs.ContainsKey(target) ? -1 : refIDs[target];

			return new(name, JsonConvert.SerializeObject(new List<int>() { id }));
		} else if (type.IsGenericType && type.IsAssignableTo(typeof(IEnumerable))) {
			List<int> ids = new();
			IEnumerable raw = (IEnumerable)value;
			foreach (object obj in raw) {
				GameObject target = (GameObject)obj;
				int id = target == null || !refIDs.ContainsKey(target) ? -1 : refIDs[target];
				ids.Add(id);
			}

			return new(name, JsonConvert.SerializeObject(ids));
		} else {
			throw new GameModelPersistenceException($"Gameobject reference with name {name} has a type that's not yet supported: {type.Name}.");
		}
	}

	public bool Load(int slot) {
		SafariSaveHead head = Saves[slot];
		GameModel model = JsonConvert.DeserializeObject<GameModel>(head.GameCoreSerialized);
		
		Jeep.Init(400);
		Tourist.Init();
		Ranger.Init();

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
				foreach (MethodInfo mi in gameobject.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
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

		foreach (SafariSaveStaticNode node in head.StaticNodes) {
			Type type = Type.GetType(node.FullTypeName);
			foreach (SafariSaveStaticProp prop in node.Props) {
				string name = prop.PropName;
				Type propType = Type.GetType(prop.FullTypeName);
				object propValue = deserMI
						.MakeGenericMethod(new[] { propType})
						.Invoke(null, new[] { prop.PropSerialized });
				if (prop.IsProp) {
					type
						.GetProperty(prop.PropName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
						.SetValue(null, propValue);
				} else {
					type.GetField(prop.PropName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
						.SetValue(null, propValue);
				}
			}
			foreach (MethodInfo mi in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) {
				if (mi.GetCustomAttribute(typeof(PostPersistenceStaticSetupAttribute)) != null) {
					Dictionary<string, List<int>> refIds = new();
					foreach (SafariSaveRefNode rn in node.Refs) {
						List<int> l = JsonConvert.DeserializeObject<List<int>>(rn.ContentSerialized);
						refIds.Add(rn.Name, l);
					}
					setupMethods.Add((null, mi, refIds));
				}
			}
		}

		foreach ((GameObject gameobject, MethodInfo mi, Dictionary<string, List<int>> refIds) in setupMethods) {
			Dictionary<string, List<GameObject>> refObjs = new();
			foreach (string key in refIds.Keys) {
				List<GameObject> current = new();
				foreach (int i in refIds[key]) {
					GameObject go = i < 0 || !refs.ContainsKey(i) ? null : refs[i];
					current.Add(go);
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
		foreach (string dir in Directory.GetDirectories(SavePath)) {
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
		.GetDirectories(SavePath)
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
