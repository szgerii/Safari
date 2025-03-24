using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Safari.Popups;

class AlertMenu : PopupMenu {
    private Header header;
    private Paragraph paragraph;
    private Button button1;
    private Button button2 = null;

    public event EventHandler<bool> Chosen;

    /// <summary>
    /// Creates a popup alert with one button. When the button is pressed Chosen event is called with the value true.
    /// </summary>
    /// <param name="header">Header text</param>
    /// <param name="text">Main body of text for the popup</param>
    /// <param name="button">Button text</param>
    public AlertMenu(string header, string text, string button = "OK") {
        PrepareUIElements(header, text);

        this.button1 = new Button(button, ButtonSkin.Default, Anchor.BottomCenter);
        this.button1.OnClick = Agree;
        this.button1.Size = new Vector2(0.4f, 0.2f);
        this.button1.Offset = new Vector2(0.1f, 0.1f);
        this.button1.Padding = new Vector2(0);
        this.button1.MaxSize = new Vector2(200, 100);

        this.panel.AddChild(this.header);
        this.panel.AddChild(this.paragraph);
        this.panel.AddChild(this.button1);
    }

    /// <summary>
    /// Creates a popup alert with two buttons. When a button is pressed Chosen event is called with a bool value.
    /// </summary>
    /// <param name="header">Header text</param>
    /// <param name="text">Main body of text for the popup</param>
    /// <param name="agreeButton">Button text for the button that returns true</param>
    /// <param name="disagreeButton">Button text for the button that returns false</param>
    public AlertMenu(string header, string text, string agreeButton, string disagreeButton) {
        PrepareUIElements(header, text);

        //Button 1 styling
        this.button1 = new Button(agreeButton, ButtonSkin.Default, Anchor.BottomLeft);
        this.button1.OnClick = Agree;
        this.button1.Size = new Vector2(0.4f, 0.2f);
        this.button1.Offset = new Vector2(0.1f, 0.1f);
        this.button1.Padding = new Vector2(0);
        this.button1.MaxSize = new Vector2(200, 100);

        //Button 2 styling
        this.button2 = new Button(disagreeButton, ButtonSkin.Default, Anchor.BottomRight);
        this.button2.OnClick = Disagree;
        this.button2.Size = new Vector2(0.4f, 0.2f);
        this.button2.Offset = new Vector2(0.1f, 0.1f);
        this.button2.Padding = new Vector2(0);
        this.button2.MaxSize = new Vector2(200, 100);

        //Adding items to the panel
        this.panel.AddChild(this.header);
        this.panel.AddChild(this.paragraph);
        this.panel.AddChild(this.button1);
        this.panel.AddChild(this.button2);
    }

    //Prepares the static elements that doesn't depend on the number of buttons.
    private void PrepareUIElements(string header, string text) {
        this.header = new Header(header, Anchor.TopCenter);
        this.header.Size = new Vector2(0f, 0.2f);
        this.header.Padding = new Vector2(0);

        this.paragraph = new Paragraph(text, Anchor.Center);
        this.paragraph.Size = new Vector2(0f, 0.6f);
        this.paragraph.Padding = new Vector2(0);
        this.paragraph.AlignToCenter = true;

        this.panel = new Panel(new Vector2(0.4f, 0.6f), PanelSkin.Default, Anchor.Center);
        this.panel.Padding = new Vector2(20,20);
        this.panel.MaxSize = new Vector2(500, 500);
    }

    private void Agree(Entity entity) {
        this.Hide();
        this.Chosen?.Invoke(this, true);
    }

    private void Disagree(Entity entity) {
        this.Hide();
        this.Chosen?.Invoke(this, false);
    }
}
