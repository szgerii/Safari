using Engine.Debug;
using Engine.Input;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Safari.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Safari.Popups;

public class DebugConsole : PopupMenu, IResettableSingleton {
	private static DebugConsole? instance;
	public static DebugConsole Instance {
		get {
			instance ??= new();
			return instance;
		}
	}
	public static void ResetSingleton() {
        instance?.Hide();
        instance = null;
	}

	private readonly Panel consoleTextPanel;
    private readonly RichParagraph consoleTextLog;
    private readonly TextInput input;
    private bool visible;
    private StringBuilder builder;
    private int scrollNeeded;
    private bool tryFocusInput = true;
    private Vector2? mousePosStorage = null;
    private readonly LinkedList<string> commandHistory = new();
    private LinkedListNode<string> currentHistoryNode = null;

    public static bool Visible => Instance.visible;

    private DebugConsole() {
        background = null;
        //initialize the main panel and set visibility to false
        visible = false;
        panel = new Panel(new Vector2(0.7f, 0.5f), PanelSkin.Default, Anchor.Center);
        panel.Padding = new Vector2(0);
        panel.MinSize = new Vector2(700, 500);
        panel.Draggable = true;

        //initialize the console panel and style it
        consoleTextPanel = new Panel(new Vector2(0f, 0.8f), PanelSkin.Default, Anchor.TopLeft);
        consoleTextPanel.Padding = new Vector2(10);
        consoleTextPanel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
        consoleTextPanel.Scrollbar.Anchor = Anchor.CenterRight;
        consoleTextPanel.Scrollbar.AdjustMaxAutomatically = true;

        //initialize the console text container
        consoleTextLog = new RichParagraph();
        consoleTextLog.Padding = new Vector2(0);

        consoleTextPanel.AddChild(this.consoleTextLog);

        //initialize the text input and style it
        input = new TextInput(true, new Vector2(0f, 60), Anchor.BottomLeft, null);
        input.MaxSize = new Vector2(0f, 100);
        input.Identifier = "debug-console-input";
        input.OnValueChange = ProcessInput;
        input.PlaceholderText = "Type here";
        input.Padding = new Vector2(10, 15);

        builder = new StringBuilder();

        //set up the main panel
        panel.AddChild(this.input);
        panel.AddChild(this.consoleTextPanel);

        scrollNeeded = 0;

        //autoscroll to the bottom
        consoleTextLog.AfterDraw = (Entity entity) => {
            if (scrollNeeded > 0) {
                consoleTextPanel.Scrollbar.Value = consoleTextPanel.Scrollbar.Max;
                --scrollNeeded;
            }
        };
    }

    /// <summary>
    /// Shows or hides the debug console. Default hotkey: F1
    /// </summary>
    public void ToggleDebugConsole() {
        if (visible) {
            input.IsFocused = false;
            base.Hide();
            visible = false;
        } else {
            base.Show();
            visible = true;
            tryFocusInput = true;
        }
    }

    /// <summary>
    /// This method provides a way to write text to the console.
    /// </summary>
    /// <param name="text">Output text</param>
    /// <param name="addPrefix">Whether to add a '>' symbol as a prefix</param>
    public void Write(string text, bool addPrefix = true) {
        builder.Append($"{(addPrefix ? "> " : "")}{text}\n");
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    /// <summary>
    /// Clears previous console output
    /// </summary>
    public void Clear() {
        builder = new();
        consoleTextLog.Text = "";
    }

    //Checks if the user had hit an Enter and if yes,
    //outputs it, tries to run the entered text as a debug command, and informs the user if it was successful.
    private void ProcessInput(Entity entity) {
        if (input.Value.Length == 0 || (input.Value.Length == 1 && (input.Value[0] == '\n' || input.Value[0] == '\r'))) {
            input.Value = "";
            return;
        }

        if (!input.Value.Contains('\n') && !input.Value.Contains('\r')) {
            return;
        }

        string consoleInput = input.Value.Replace("\n", "").Replace("\r", "").ToLower();

        Write(consoleInput);

        if (RunDebugCustomCommands(consoleInput)) {

        } else if (DebugMode.HasExecutedFeature(consoleInput)) {
            DebugMode.Execute(consoleInput);
            Confirm($"{consoleInput} executed successfully");
        } else if (DebugMode.HasLoopedFeature(consoleInput)) {
            DebugMode.ToggleFeature(consoleInput);
            Confirm($"{consoleInput} toggled successfully");
        } else {
            Error($"{consoleInput} is not a command");
        }

        if (commandHistory.First?.Value != consoleInput) {
            commandHistory.AddFirst(consoleInput);
        }
        currentHistoryNode = null;

        input.Value = "";
    }

    //checks if the input is a custom debug console command and runs it if yes
    private bool RunDebugCustomCommands(string input) {
        if (TryRunFlagCommand(input)) return true;

        switch (input) {
            case "help":
                Help();
                return true;
            case "clear":
                Clear();
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// This method provides a way to write an error message to the console.
    /// </summary>
    /// <param name="text">Error message text</param>
    public void Error(string text) {
        builder.Append("{{RED}}").Append(text).Append("{{DEFAULT}}").Append('\n');
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    /// <summary>
    /// This method provides a way for the system to confirm actions.
    /// </summary>
    /// <param name="text">Confirmation text</param>
    public void Confirm(string text) {
        builder.Append("{{L_GREEN}}").Append(text).Append("{{DEFAULT}}").Append('\n');
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    /// <summary>
    /// This method provides a way to write an info message to the console.
    /// </summary>
    /// <param name="text">Info message text</param>
    public void Info(string text) {
        builder.Append("{{L_BLUE}}").Append(text).Append("{{DEFAULT}}").Append('\n');
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    // geonbit power ups & window closing
    public override void Update(GameTime gameTime) {
        if (tryFocusInput) {
            if (mousePosStorage == null) {
                Rectangle inputDestRect = input.GetActualDestRect();

                if (!inputDestRect.IsEmpty) {
                    mousePosStorage = UserInterface.Active.MouseInputProvider.MousePosition;
                    UserInterface.Active.MouseInputProvider.UpdateMousePosition(inputDestRect.Center.ToVector2());
                    UserInterface.Active.MouseInputProvider.DoClick();
                }
            } else {
                UserInterface.Active.MouseInputProvider.UpdateMousePosition(mousePosStorage.Value);
                mousePosStorage = null;
                tryFocusInput = false;
            }
        }

        input.IsFocused = UserInterface.Active.ActiveEntity == input;

        if (InputManager.IsGameFocused) return;

        if (input.IsFocused) {
            if (JustPressed(Keys.Up)) {
                if (currentHistoryNode == null) {
                    currentHistoryNode = commandHistory.First;
                } else if (currentHistoryNode.Next != null) {
                    currentHistoryNode = currentHistoryNode.Next;
                }

                input.Value = currentHistoryNode?.Value ?? "";
                input.Caret = -1;
            } else if (JustPressed(Keys.Down)) {
                if (currentHistoryNode != null) {
                    currentHistoryNode = currentHistoryNode.Previous;
                }

                input.Value = currentHistoryNode?.Value ?? "";
                input.Caret = -1;
            }
        }

        if (JustPressed(Keys.F1)) {
            ToggleDebugConsole();
            UserInterface.Active.MouseInputProvider.DoClick();
        }
    }

    //prints out the available debug commands
    private void Help() {
        Info("{{BOLD}}Execute commands:{{DEFAULT}}");
        foreach (ExecutedDebugFeature item in DebugMode.ExecutedFeatures) {
            Info($"{item.Name}");
        }
        SkipLine();
        Info("{{BOLD}}Toggle commands:{{DEFAULT}}");
        foreach (LoopedDebugFeature item in DebugMode.LoopedFeatures) {
            Info($"{item.Name}");
        }
        SkipLine();
        Info("{{BOLD}}Debug console commands:{{DEFAULT}}");
        Info("help");
        Info("clear");
    }

    private bool TryRunFlagCommand(string input) {
        string[] inputArgs = input
            .Split(' ')
            .Select(arg => arg.Trim())
            .Where(arg => arg != "")
            .ToArray();

        if (inputArgs.Length == 0) return false;

        if (inputArgs.Length != 2) {
            switch (inputArgs[0]) {
                case "enable":
                    Error("Usage: enable <flag>");
                    return true;

                case "disable":
                    Error("Usage: disable <flag>");
                    return true;

                case "toggle":
                    Error("Usage: toggle <flag>");
                    return true;

                case "flagval":
                    Error("Usage: flagval <flag>");
                    return true;

                default:
                    return false;
            }
        }

        switch (inputArgs[0]) {
            case "enable":
                DebugMode.EnableFlag(inputArgs[1]);
                Confirm($"Successfully enabled flag '{inputArgs[1]}'");
                return true;

            case "disable":
                DebugMode.DisableFlag(inputArgs[1]);
                Confirm($"Successfully disabled flag '{inputArgs[1]}'");
                return true;

            case "toggle":
                DebugMode.ToggleFlag(inputArgs[1]);
                Confirm($"Successfully toggled flag '{inputArgs[1]}' to {DebugMode.IsFlagActive(inputArgs[1])}");
                return true;

            case "flagval":
                bool defined = DebugMode.HasFlagBeenSet(inputArgs[1]);
                if (defined) {
                    Confirm($"Flag value of '{inputArgs[1]}': {DebugMode.IsFlagActive(inputArgs[1])}");
                } else {
                    Confirm($"Flag '{inputArgs[1]}' hasn't been defined yet");
                }

                return true;

            default:
                return false;
        }

	}

    //Scrolls the text log to the bottom.
    private void ScrollConsoleDown() {
        scrollNeeded = 3;
    }

    //skips a line in the console, without the > char
    private void SkipLine() {
        builder.Append('\n');
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    private bool JustPressed(Keys key) {
        bool wasUp = InputManager.Keyboard.PrevKS.IsKeyUp(key);
        bool isDown = InputManager.Keyboard.CurrentKS.IsKeyDown(key);

        return wasUp && isDown;
    }
}
