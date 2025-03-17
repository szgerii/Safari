using Engine.Debug;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Text;

namespace Safari.Popups;
class DebugConsole : PopupMenu {
    private static DebugConsole instance = new DebugConsole();
    private Panel consoleTextPanel;
    private RichParagraph consoleTextLog;
    private TextInput input;
    private bool visible;
    private StringBuilder builder;

    /// <summary>
    /// This provides accessibility to the debug console and its methods.
    /// </summary>
    public static DebugConsole Instance => instance;

    private DebugConsole() {
        //initialize the main panel and set visibility to false
        visible = false;
        panel = new Panel(new Vector2(0.7f, 0.5f), PanelSkin.Default, Anchor.TopLeft);
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
        input = new TextInput(true, new Vector2(0f, 0.15f), Anchor.BottomLeft, null);
        input.Identifier = "debug-console-input";
        input.OnValueChange = ProccesInput;
        input.PlaceholderText = "Type here";
        input.Padding = new Vector2(20, 10);

        builder = new StringBuilder();

        //set up the main panel
        panel.AddChild(this.input);
        panel.AddChild(this.consoleTextPanel);
    }

    /// <summary>
    /// Shows or hides the debug console. Default hotkey: F1
    /// </summary>
    public void ToggleDebugConsole() {
        if (visible) {
            base.Hide();
            visible = false;
        } else {
            base.Show();
            visible = true;
        }
    }

    /// <summary>
    /// This method provides a way to write text to the console.
    /// </summary>
    /// <param name="text">Output text</param>
    public void Write(string text) {
        builder.Append($"> {text}\n");
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    //Checks if the user had hit an Enter and if yes,
    //outputs it, tries to run the entered text as a debug command, and informs the user if it was successful.
    private void ProccesInput(Entity entity) {
        if (input.Value.Length == 0 || (input.Value.Length == 1 && (input.Value[0] == '\n' || input.Value[0] == '\r'))) {
            input.Value = "";
            return;
        }

        if (input.Value[input.Value.Length - 1] != '\n' && input.Value[input.Value.Length - 1] != '\r') {
            return;
        }

        string consoleInput = input.Value.TrimEnd('\r', '\n').ToLower();

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

        ScrollConsoleDown();
        input.Value = "";
    }

    //checks if the input is a custom debug console command and runs it if yes
    private bool RunDebugCustomCommands(string input) {
        switch (input) {
            case "help":
                Help();
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
        builder.Append("{{RED}}").Append(text).Append("{{DEFAULT}}").Append("\n");
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    /// <summary>
    /// This method provides a way for the system to confirm actions.
    /// </summary>
    /// <param name="text">Confirmation text</param>
    public void Confirm(string text) {
        builder.Append("{{L_GREEN}}").Append(text).Append("{{DEFAULT}}").Append("\n");
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }

    private void Info(string text) {
        builder.Append("{{L_BLUE}}").Append(text).Append("{{DEFAULT}}").Append("\n");
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
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
    }

    //Scrolls the text log to the bottom.
    private void ScrollConsoleDown() {
        consoleTextPanel.Scrollbar.Value = consoleTextPanel.Scrollbar.Max;
    }

    //skips a line in the console, without the > char
    private void SkipLine() {
        builder.Append("\n");
        consoleTextLog.Text = builder.ToString();
        ScrollConsoleDown();
    }
}
