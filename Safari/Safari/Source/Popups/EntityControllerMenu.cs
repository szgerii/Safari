using Engine.Components;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Scenes;
using System;

namespace Safari.Popups;

public class EntityControllerMenu : PopupMenu {
	public static EntityControllerMenu Active { get; private set; } = null;
	private Button closeButton;
	private readonly Image image;
	private Header header;
	private Rectangle maskArea;
	protected Safari.Model.Entities.Entity controlledEntity;

	public EntityControllerMenu(Safari.Model.Entities.Entity entity) {
		PrepareUI();

		header.Text = entity.DisplayName;
		controlledEntity = entity;
		if (controlledEntity.Sprite is AnimatedSpriteCmp anim) {
			image = new Image(anim.Texture.ToTexture2D(), new Vector2(128, 128), ImageDrawMode.Stretch, Anchor.AutoCenter);
			image.Offset = new Vector2(0, 0.3f);
			UpdateSourceRectangle();
			panel.AddChild(image);
		}
	}

	private Vector2? prevOffset = null;
	private void PrepareUI() {
		background = null;
		panel = new Panel(new Vector2(0.3f, 0.9f), PanelSkin.Default, Anchor.TopLeft, prevOffset ?? new Vector2(16, 16));
		panel.Tag = "PassiveFocus";
		panel.Padding = new Vector2(20, 20);

		closeButton = new Button("Close", ButtonSkin.Default, Anchor.BottomCenter);
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

	private void OnCloseClick(Entity e) {
		Hide();
	}

	private void OnEntityDied(object sender, EventArgs e) {
		Hide();
	}

	public override void Show() {
		if (Active != null) {
			Active.Hide();
		}
		Active = this;
		closeButton.OnClick += OnCloseClick;
		controlledEntity.Died += OnEntityDied;
		controlledEntity.IsBeingInspected = true;
		base.Show();
		maskArea = this.panel.CalcDestRect();

		GameScene.Active.MaskedAreas.Add(maskArea);
	}

	public override void Hide() {
		closeButton.OnClick -= OnCloseClick;
		controlledEntity.Died -= OnEntityDied;
		controlledEntity.IsBeingInspected = false;
		base.Hide();
		Active = null;
		
		GameScene.Active.MaskedAreas.Remove(maskArea);
	}

	public override void Update(GameTime gameTime) {
		UpdateSourceRectangle();
		UpdateData();

		base.Update(gameTime);
	}

	protected virtual void UpdateSourceRectangle() {
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
	}

	protected virtual void UpdateData() { }
}
