using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Safari.Popups;

public class AlertMenu : PopupMenu {
    private Header header;
    private Paragraph paragraph;
    private readonly Button button1;
    private readonly Button button2 = null;
    private static readonly Queue<AlertMenu> queue = new Queue<AlertMenu>();
    private static int waitCounter = 0;
    private static bool waitNeeded = false;
    private static AlertMenu Active = null;
    private static bool nextAlertShowable = false;

    /// <summary>
    /// Returns the user's choice.
    /// </summary>
    public event EventHandler<bool> Chosen;

    /// <summary>
    /// Creates a popup alert with one button. When the button is pressed Chosen event is called with the value true.
    /// </summary>
    /// <param name="header">Header text</param>
    /// <param name="text">Main body of text for the popup</param>
    /// <param name="button">Button text</param>
    public AlertMenu(string headerText, string text, string button = "OK") {
        PrepareUIElements(headerText, text);

        button1 = new Button(button, ButtonSkin.Default, Anchor.BottomCenter);
        button1.OnClick = Agree;
        button1.Size = new Vector2(0.4f, 0.2f);
        button1.Offset = new Vector2(0.1f, 0.1f);
        button1.Padding = new Vector2(0);
        button1.MaxSize = new Vector2(200, 100);

        panel.AddChild(header);
        panel.AddChild(paragraph);
        panel.AddChild(button1);
    }

    /// <summary>
    /// Creates a popup alert with two buttons. When a button is pressed Chosen event is called with a bool value.
    /// </summary>
    /// <param name="header">Header text</param>
    /// <param name="text">Main body of text for the popup</param>
    /// <param name="agreeButton">Button text for the button that returns true</param>
    /// <param name="disagreeButton">Button text for the button that returns false</param>
    public AlertMenu(string headerText, string text, string agreeButton, string disagreeButton) {
        PrepareUIElements(headerText, text);

        //Button 1 styling
        button1 = new Button(agreeButton, ButtonSkin.Default, Anchor.BottomLeft);
        button1.OnClick = Agree;
        button1.Size = new Vector2(0.4f, 0.2f);
        button1.Offset = new Vector2(0.1f, 0.1f);
        button1.Padding = new Vector2(0);
        button1.MaxSize = new Vector2(200, 100);

        //Button 2 styling
        button2 = new Button(disagreeButton, ButtonSkin.Default, Anchor.BottomRight);
        button2.OnClick = Disagree;
        button2.Size = new Vector2(0.4f, 0.2f);
        button2.Offset = new Vector2(0.1f, 0.1f);
        button2.Padding = new Vector2(0);
        button2.MaxSize = new Vector2(200, 100);

        //Adding items to the panel
        panel.AddChild(paragraph);
        panel.AddChild(header);
        panel.AddChild(button1);
        panel.AddChild(button2);
    }

    //Prepares the static elements that doesn't depend on the number of buttons.
    private void PrepareUIElements(string headerText, string text) {
        header = new Header(headerText, Anchor.TopCenter);
        header.Size = new Vector2(0f, 0.2f);
        header.Padding = new Vector2(0);
        header.AlignToCenter = true;

        paragraph = new Paragraph(text, Anchor.Center);
        paragraph.Size = new Vector2(0f, 0.6f);
        paragraph.Padding = new Vector2(0);
        paragraph.AlignToCenter = true;

        panel = new Panel(new Vector2(0.4f, 0.6f), PanelSkin.Default, Anchor.Center);
        panel.Padding = new Vector2(20, 20);
        panel.MaxSize = new Vector2(500, 500);
    }

    private void Agree(Entity entity) {
        Hide();
        Chosen?.Invoke(this, true);
    }

    private void Disagree(Entity entity) {
        Hide();
        Chosen?.Invoke(this, false);
    }

    public override void Show() {
        if (Active == null) {
            Active = this;
            base.Show();
        } else {
            queue.Enqueue(this);
        }
    }

    public override void Hide() {
        base.Hide();
        nextAlertShowable = true;
        Active = null;
    }

    //shows the next alert in the queue
    private static void ShowNextAlert() {
        if (Active == null && queue.TryDequeue(out AlertMenu nextAlert)) {
            nextAlert.Show();
        }
    }

    /// <summary>
    /// Handles the update logic, and alert queue.
    /// </summary>
    public static void Adjust() {
        if (Active != null) {
            return;
        }
        if (nextAlertShowable) {
            nextAlertShowable = false;
            if (queue.Count != 0) {
                waitCounter = 2;
                waitNeeded = true;
            }
        }
        if (waitNeeded) {
            if (waitCounter > 0) {
                --waitCounter;
                return;
            } else {
                waitCounter = 0;
                waitNeeded = false;
                ShowNextAlert();
            }
        }
    }
}
