using Engine.Input;
using GeonBit.UI.DataTypes;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Model.Entities.Animals;
using System;

namespace Safari.Popups;

public class RangerControllerMenu : EntityControllerMenu {
	readonly DropDown dropDown;
	readonly Button fireButton;
	readonly Array animalTypes = Enum.GetValues(typeof(AnimalSpecies));
	readonly Safari.Model.Entities.Ranger ranger;
	readonly int nullValue = -1;

	public RangerControllerMenu(Safari.Model.Entities.Ranger ranger) : base(ranger) {
		this.ranger = ranger;
		dropDown = new DropDown(new Vector2(0.9f, 0.6f), Anchor.AutoCenter, null, PanelSkin.Simple, PanelSkin.Simple);
		dropDown.SelectList.Scale = 0.5f;
		foreach (int value in animalTypes) {
			dropDown.AddItem(((AnimalSpecies)value).GetDisplayName(), value);
			if (value > nullValue) {
				nullValue = value;
			}
		}
		nullValue++;
		dropDown.AddItem("Default", nullValue);
		dropDown.SelectedIndex = ranger.TargetSpecies != null ? (int)ranger.TargetSpecies : nullValue;
		
		panel.AddChild(dropDown);

        StyleProperty deleteBase = new StyleProperty(Color.Red);
        StyleProperty deleteHover = new StyleProperty(Color.DarkRed);
        StyleProperty deleteClick = new StyleProperty(Color.IndianRed);

        fireButton = new Button("Fire", ButtonSkin.Default, Anchor.AutoCenter);
		fireButton.Size = new Vector2(0.8f, 0.1f);
		fireButton.ButtonParagraph.Scale = 0.75f;
        fireButton.SetStyleProperty("FillColor", deleteBase, EntityState.Default);
        fireButton.SetStyleProperty("FillColor", deleteHover, EntityState.MouseHover);
        fireButton.SetStyleProperty("FillColor", deleteClick, EntityState.MouseDown);
        panel.AddChild(fireButton);
	}

	public override void Show() {
		dropDown.OnValueChange += OnDropDownChanged;
		fireButton.OnClick += OnFireBtnClick;
		base.Show();
		InputManager.Mouse.ScrollLock = true;
	}

	public override void Hide() {
		dropDown.OnValueChange -= OnDropDownChanged;
		fireButton.OnClick -= OnFireBtnClick;
		base.Hide();
		InputManager.Mouse.ScrollLock = false;
	}

	protected override void UpdateData() {
		dropDown.SelectedIndex = ranger.TargetSpecies != null ? (int)ranger.TargetSpecies : nullValue;
	}

	private void OnDropDownChanged(Entity e) {
		if (dropDown.SelectedIndex >= nullValue) {
			ranger.TargetSpecies = null;
		} else {
			ranger.TargetSpecies = (AnimalSpecies)dropDown.SelectedIndex;
		}
	}

	private void OnFireBtnClick(Entity e) {
		this.ranger.Fire();
	}
}
