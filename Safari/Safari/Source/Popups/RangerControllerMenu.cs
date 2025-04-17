using Engine.Input;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Animals;
using System;

namespace Safari.Popups;

public class RangerControllerMenu : EntityControllerMenu {
	DropDown dropDown;
	Button fireButton;
	Array animalTypes = Enum.GetValues(typeof(AnimalSpecies));
	Safari.Objects.Entities.Ranger ranger;
	int nullValue = -1;

	public RangerControllerMenu(Safari.Objects.Entities.Ranger ranger) : base(ranger) {
		this.ranger = ranger;
		dropDown = new DropDown(new Vector2(0.9f, 0.6f), Anchor.AutoCenter, null, PanelSkin.Simple, PanelSkin.Simple);
		dropDown.SelectList.Scale = 0.5f;
		foreach (int value in animalTypes) {
			dropDown.AddItem(((AnimalSpecies)value).ToString(), value);
			if (value > nullValue) {
				nullValue = value;
			}
		}
		nullValue++;
		dropDown.AddItem("Default", nullValue);
		dropDown.SelectedIndex = ranger.TargetSpecies != null ? (int)ranger.TargetSpecies : nullValue;
		
		panel.AddChild(dropDown);

		fireButton = new Button("Fire", ButtonSkin.Default, Anchor.AutoCenter);
		fireButton.Size = new Vector2(0.8f, 0.1f);
		fireButton.ButtonParagraph.Scale = 0.75f;
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
