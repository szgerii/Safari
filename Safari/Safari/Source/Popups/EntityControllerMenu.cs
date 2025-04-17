using Engine.Components;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Safari.Popups;

public class EntityControllerMenu : PopupMenu {
	public static EntityControllerMenu Active { get; private set; } = null;
	private Button closeButton;
	private Image image;
	private Header header;
	private bool visible = false;
	protected Safari.Objects.Entities.Entity controlledEntity;

	public EntityControllerMenu(Safari.Objects.Entities.Entity entity) {
		PrepareUI();

		header.Text = entity.DisplayName;
		controlledEntity = entity;
		controlledEntity.Died += (object sender, EventArgs e) => {
			Hide();
		};
		if (controlledEntity.Sprite is AnimatedSpriteCmp anim) {
			image = new Image(anim.Texture, new Vector2(128, 128), ImageDrawMode.Stretch, Anchor.AutoCenter);
			image.Offset = new Vector2(0, 0.3f);
			panel.AddChild(image);
		}
	}
	// TODO UI bug: click through panel
	// TODO UI bug: multiple hides possible, lifecycle, events????, updates???
	// TODO mouse bug: click target is really messed
	private void PrepareUI() {
		background = null;
		panel = new Panel(new Vector2(0.3f, 0.8f), PanelSkin.Default, Anchor.TopRight, new Vector2(16, 16));
		panel.Tag = "PassiveFocus";
		panel.Padding = new Vector2(20, 20);

		closeButton = new Button("Close", ButtonSkin.Default, Anchor.BottomCenter);
		closeButton.OnClick = (e) => {
			Hide();
		};
		closeButton.Size = new Vector2(0.6f, 0.1f);
		closeButton.Offset = new Vector2(0.1f, 0.1f);
		closeButton.Padding = new Vector2(0);
		closeButton.MaxSize = new Vector2(200, 100);
		panel.AddChild(closeButton);

		header = new Header("", Anchor.TopCenter);
		header.Size = new Vector2(0f, 0.2f);
		header.Padding = new Vector2(0);
		header.AlignToCenter = true;
		panel.AddChild(header);
	}

	public override void Show() {
		if (!visible) {
			if (Active != null) {
				Active.Hide();
			}
			Active = this;
			base.Show();
			visible = true;
		}
	}

	public override void Hide() {
		if (visible) {
			base.Hide();
			Active = null;
			visible = false;
		}
	}

	public override void Update(GameTime gameTime) {
		if (controlledEntity.Sprite is AnimatedSpriteCmp anim) {
			Rectangle r = anim.CalculateSrcRec();
			
			image.SourceRectangle = r;
			if (r.Width >= r.Height) {
				float width = 96;
				float height = (width / r.Size.X) * r.Size.Y;
				image.Size = new Vector2(width, height);
			} else {
				float height = 96;
				float width = (height / r.Size.Y) * r.Size.X;
				image.Size = new Vector2(width, height);
			}
		} else {
			image.SourceRectangle = null;
		}
		base.Update(gameTime);
	}
}
