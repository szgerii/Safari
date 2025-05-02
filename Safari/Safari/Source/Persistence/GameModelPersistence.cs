using Safari.Model;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Safari.Scenes;
using Engine.Scenes;
using Engine.Graphics.Stubs.Texture;
using Safari.Model.Entities.Tourists;
using Engine.Objects;
using Engine;

namespace Safari.Source.Persistence;

/// <summary>
/// Handles saving and loading the game models state to / from a .safsav file
/// </summary>
public class GameModelPersistence {
	public const int MAX_SLOTS = 5;
	public const string SAVE_PATH = "saves";

	private string dirPath;
	private List<SafariSaveHead> saves = new();

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
		SafariSaveHead head = new SafariSaveHead(modelStr, metadata);
		using (StreamWriter sw = new StreamWriter(fileName)) {
			sw.WriteLine(JsonConvert.SerializeObject(head));
		}
	}

	public bool Load(int slot) {
		GameModel model = JsonConvert.DeserializeObject<GameModel>(saves[slot].GameCoreSerialized);
		GameScene scene = new GameScene(model);
		model.Level.Background = Game.CanDraw ? Game.LoadTexture("Assets/Background/Background") : new NoopTexture2D(null, 3584, 2048);
		SceneManager.Load(scene);
		scene.AddObject(model.Level);
		Jeep.Init(400);
		Tourist.Init();
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
