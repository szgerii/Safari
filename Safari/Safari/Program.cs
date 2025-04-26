using Safari;
using System;
using System.Linq;

bool headless = args.Contains("--headless");

GameStartupMode startup = GameStartupMode.Default;
if (args.Contains("--demo")) {
	startup = GameStartupMode.DemoScene;
} else if (args.Contains("--empty")) {
	startup = GameStartupMode.EmptyScene;
} else if (args.Contains("--main-menu")) {
	startup = GameStartupMode.MainMenu;
}

using var game = new Game(headless) { StartupMode = startup };
game.Run();
