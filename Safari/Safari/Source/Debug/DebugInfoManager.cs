using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Safari.Debug;

public static class DebugInfoManager {

	private static readonly List<DebugInfo> debugInfos = new();
	private static readonly Dictionary<DebugInfoPosition, Paragraph> infoParagraphs = new();

	static DebugInfoManager() {
		if (Game.Instance?.IsHeadless ?? true) return;

		foreach (DebugInfoPosition pos in Enum.GetValues(typeof(DebugInfoPosition))) {
			Paragraph p = new Paragraph();
			p.ClickThrough = true;
			p.Offset = new Vector2(15);

			switch (pos) {
				case DebugInfoPosition.TopLeft:
					p.Anchor = Anchor.TopLeft;
					break;
				case DebugInfoPosition.TopRight:
					p.Anchor = Anchor.TopRight;
					break;
				case DebugInfoPosition.BottomLeft:
					p.Anchor = Anchor.BottomLeft;
					break;
				case DebugInfoPosition.BottomRight:
					p.Anchor = Anchor.BottomRight;
					break;
			}

			infoParagraphs[pos] = p;
		}
	}

	/// <summary>
	/// Adds a line of information to the debug output
	/// (infos need to be added every frame)
	/// </summary>
	/// <param name="info">The DebugInfo object to add</param>
	public static void AddInfo(DebugInfo info) {
		debugInfos.Add(info);
	}

	/// <summary>
	/// Shorthand for creating and adding a DebugInfo object
	/// </summary>
	/// <param name="info">The body of the DebugInfo</param>
	public static void AddInfo(string info) => AddInfo(new DebugInfo() { Info = info });

	/// <summary>
	/// Shorthand for creating and adding a DebugInfo object
	/// </summary>
	/// <param name="name">The name of the DebugInfo, which will be prepended to the line</param>
	/// <param name="info">The body of the DebugInfo</param>
	/// <param name="pos">The position to display the line at (defaults to top left)</param>
	public static void AddInfo(string name, string info, DebugInfoPosition? pos = null) {
		DebugInfo di = new DebugInfo { Info = info, Name = name };

		if (pos != null) {
			di.ScreenPos = pos.Value;
		}

		AddInfo(di);
	}

	/// <summary>
	/// Removes a previously added DebugInfo
	/// (a DebugInfo only needs to be removed if you already added it in the current frame)
	/// </summary>
	/// <param name="info">The DebugInfo to remove</param>
	public static void RemoveInfo(DebugInfo info) {
		debugInfos.Remove(info);
	}

	/// <summary>
	/// Turn on debug info rendering on the active UI
	/// </summary>
	public static void ShowInfos() {
		foreach (Paragraph p in infoParagraphs.Values) {
			p.Tag = "PassiveFocus";
			UserInterface.Active.AddEntity(p);
		}
	}

	/// <summary>
	/// Turn off debug info rendering on the active UI
	/// </summary>
	public static void HideInfos() {
		foreach (Paragraph p in infoParagraphs.Values) {
			UserInterface.Active.RemoveEntity(p);
		}
	}

	/// <summary>
	/// Needs to be called before every Update
	/// <br />
	/// NOTE: this is called before any Update logic happens, it's not the same as the <see cref="Engine.Scenes.Scene.PreUpdate"/> hook in Scenes
	/// </summary>
	public static void PreUpdate() {
		debugInfos.Clear();
	}

	/// <summary>
	/// Needs to be called before every frame GeonBit's Draw
	/// <br />
	/// NOTE: this si called after every Draw logic has finished, it's not the same as the <see cref="Engine.Scenes.Scene.PreDraw"/> hook in Scenes
	/// </summary>
	public static void PreDraw() {
		foreach (DebugInfoPosition pos in Enum.GetValues(typeof(DebugInfoPosition))) {
			infoParagraphs[pos].Text = "";
		}

		foreach (DebugInfo info in debugInfos) {
			Paragraph p = infoParagraphs[info.ScreenPos];

			if (p.Text != "") {
				p.Text += "\n";
			}

			p.Text += info.ToString();
		}
	}
}
