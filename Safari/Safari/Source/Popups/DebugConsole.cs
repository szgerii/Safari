using Engine.Debug;
using Engine.Input;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Debug;
using System.Collections.Generic;
using System.Linq;

namespace Safari.Popups;
class DebugConsole : PopupMenu {
    private static DebugConsole instance = new DebugConsole();
    private Panel consoleTextPanel;
    private Paragraph consoleText;
    private TextInput input;
    private Button closeButton;
    private bool visible;

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
        this.consoleText = new Paragraph("");
        this.consoleText.Padding = new Vector2(0);

        this.consoleTextPanel.AddChild(this.consoleText);

        //initialize the text input and style it
        this.input = new TextInput(true, new Vector2(0.9f, 0.2f), Anchor.BottomLeft, null);
        this.input.Identifier = "debug-console-input";
        this.input.OnValueChange = ProccesInput;
        this.input.PlaceholderText = "Text here";

        //initialize the close button and style it
        this.closeButton = new Button("", ButtonSkin.Default, Anchor.BottomRight, new Vector2(0.1f,0.2f));
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

    public void ToggleDebugConsole() {
        if (visible) {
            UserInterface.Active.RemoveEntity(this.panel);
            visible = false;
        } else {
            UserInterface.Active.AddEntity(this.panel);
            visible = true;
        }
    }

    private void ScrollConsoleDown() {
        this.consoleTextPanel.Scrollbar.Value = this.consoleTextPanel.Scrollbar.Max+50;
    }

    public void WriteToConsole(string text) {
        this.consoleText.Text += "> ";
        this.consoleText.Text += text;
        this.consoleText.Text += '\n';
        ScrollConsoleDown();
    }
    
    private void ProccesInput(Entity entity) {
        if (this.input.Value.Length == 0) {
            return;
        }
        if(this.input.Value[this.input.Value.Length-1] == '\n' || this.input.Value[this.input.Value.Length - 1] == '\r') {
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
