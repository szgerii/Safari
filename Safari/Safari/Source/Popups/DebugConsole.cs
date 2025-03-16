using Engine.Debug;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;

namespace Safari.Popups;
class DebugConsole : PopupMenu {
    private static DebugConsole instance = new DebugConsole();
    private Panel consoleTextPanel;
    private Paragraph consoleTextLog;
    private TextInput input;
    private Button closeButton;
    private bool visible;

    /// <summary>
    /// This provides accessibility to the debug console and its methods.
    /// </summary>
    public static DebugConsole Instance => instance;

    private DebugConsole() {
        //initialize the main panel and set visibility to false
        this.visible = false;
        this.panel = new Panel(new Vector2(0.7f, 0.5f), PanelSkin.Default, Anchor.TopLeft);
        this.panel.Padding = new Vector2(0);
        this.panel.MinSize = new Vector2(700, 500);

        //initialize the console panel and style it
        this.consoleTextPanel = new Panel(new Vector2(0f, 0.8f), PanelSkin.Default, Anchor.TopLeft);
        this.consoleTextPanel.Padding = new Vector2(10);
        this.consoleTextPanel.PanelOverflowBehavior = PanelOverflowBehavior.VerticalScroll;
        this.consoleTextPanel.Scrollbar.Anchor = Anchor.CenterRight;
        this.consoleTextPanel.Scrollbar.AdjustMaxAutomatically = true;

        //initialize the console text container
        this.consoleTextLog = new Paragraph("");
        this.consoleTextLog.Padding = new Vector2(0);

        this.consoleTextPanel.AddChild(this.consoleTextLog);

        //initialize the text input and style it
        this.input = new TextInput(true, new Vector2(0.9f, 0.2f), Anchor.BottomLeft, null);
        this.input.Identifier = "debug-console-input";
        this.input.OnValueChange = ProccesInput;
        this.input.PlaceholderText = "Type here";

        //initialize the close button and style it
        this.closeButton = new Button("", ButtonSkin.Default, Anchor.BottomRight, new Vector2(0.1f, 0.2f));
        this.closeButton.Padding = new Vector2(0);
        this.closeButton.ClearChildren();
        Paragraph p = new Paragraph("X", Anchor.Center, new Vector2(0));
        this.closeButton.AddChild(p);
        this.closeButton.ButtonParagraph = p;
        this.closeButton.OnClick = (Entity entity) => { this.ToggleDebugConsole(); };
        this.closeButton.FillColor = Color.Red;

        //set up the main panel
        this.panel.AddChild(this.closeButton);
        this.panel.AddChild(this.input);
        this.panel.AddChild(this.consoleTextPanel);
    }

    /// <summary>
    /// Shows or hides the debug console. Default hotkey: F1
    /// </summary>
    public void ToggleDebugConsole() {
        if (visible) {
            UserInterface.Active.RemoveEntity(this.panel);
            visible = false;
        } else {
            UserInterface.Active.AddEntity(this.panel);
            visible = true;
        }
    }

    //Scrolls the text log to the bottom.
    private void ScrollConsoleDown() {
        this.consoleTextPanel.Scrollbar.Value = this.consoleTextPanel.Scrollbar.Max + 50;
    }

    /// <summary>
    /// This method provies a way to write text to the console.
    /// </summary>
    /// <param name="text">The text you want to output to the console.</param>
    public void WriteToConsole(string text) {
        this.consoleTextLog.Text += "> ";
        this.consoleTextLog.Text += text;
        this.consoleTextLog.Text += '\n';
        ScrollConsoleDown();
    }

    //Checks if the user had hit an Enter and if yes, outputs it and tries to run the entered text as a debug command.
    private void ProccesInput(Entity entity) {
        if (this.input.Value.Length == 0) {
            return;
        }
        if (this.input.Value[this.input.Value.Length - 1] == '\n' || this.input.Value[this.input.Value.Length - 1] == '\r') {
            string input = this.input.Value.TrimEnd('\r', '\n');
            WriteToConsole(input);
            ScrollConsoleDown();

            try {
                DebugMode.Execute(input);
            } catch {

            }
            try {
                DebugMode.ToggleFeature(input);
            } catch {

            }

            this.input.Value = "";
        }
    }
}
