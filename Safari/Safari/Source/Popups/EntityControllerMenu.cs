using Engine.Input;
using Engine.Components;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Safari.Popups;

public class EntityControllerMenu : PopupMenu {
	protected static readonly Vector2 BASE_SIZE = new(0.3f, 0.9f);

	public static EntityControllerMenu Active { get; private set; } = null;
	protected Button closeButton;
	protected readonly Image image;
	protected Header header;
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
		panel = new Panel(BASE_SIZE, PanelSkin.Default, Anchor.TopLeft, prevOffset ?? new Vector2(16, 16));
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
		InputManager.Mouse.ScrollLock = true;
	}

	public override void Hide() {
		closeButton.OnClick -= OnCloseClick;
		controlledEntity.Died -= OnEntityDied;
		controlledEntity.IsBeingInspected = false;
		base.Hide();
		Active = null;
		InputManager.Mouse.ScrollLock = false;
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

	private readonly Type[] ROUNDED_TYPES = [typeof(float), typeof(double)];
	private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
	protected virtual string DumpObjectDataToString(object targetObj, Type targetType = null) {
		if (targetType == null) {
			targetType = targetObj.GetType();
		}

		List<MemberInfo> infos = [.. targetType.GetProperties(FLAGS)];
		infos.AddRange(targetType.GetFields(FLAGS));

		StringBuilder sb = new("");
		foreach (MemberInfo info in infos) {
			string line = $"{info.Name}: ";

			if (info is FieldInfo field) {
				line += ROUNDED_TYPES.Contains(field.FieldType) ? $"{field.GetValue(targetObj):0.00}" : $"{field.GetValue(targetObj)}";
			} else if (info is PropertyInfo prop) {
				line += ROUNDED_TYPES.Contains(prop.PropertyType) ? $"{prop.GetValue(targetObj):0.00}" : $"{prop.GetValue(targetObj)}";
			}

			sb.Append(line);
			sb.Append('\n');
		}

		return sb.ToString();
	}
}
