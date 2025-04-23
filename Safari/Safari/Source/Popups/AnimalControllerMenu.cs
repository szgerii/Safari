using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Animals;
using Safari.Scenes;
using Safari.Scenes.Menus;

namespace Safari.Popups;
	
public class AnimalControllerMenu : EntityControllerMenu {
	private Button sellButton;
	private Button chipButton = null;
	private Paragraph chipParagraph = null;
	private Paragraph dataParagraph = null;
	private Animal animal;

	public AnimalControllerMenu(Animal animal) : base(animal) {
		this.animal = animal;

		dataParagraph = new Paragraph("");
		dataParagraph.AlignToCenter = true;
		dataParagraph.Padding = new Vector2(0, 16);
		dataParagraph.Scale = 0.97f;
		panel.AddChild(dataParagraph);

		sellButton = new Button($"Sell (+{animal.Price})", ButtonSkin.Default, Anchor.AutoCenter);
		sellButton.Size = new Vector2(0.8f, 0.1f);
		sellButton.ButtonParagraph.Scale = 0.75f;
		panel.AddChild(sellButton);

		if (animal.HasChip) {
			//chipParagraph = new Paragraph("This animal already has a microchip installed");
			//panel.AddChild(chipParagraph);
		} else {
			chipButton = new Button("Buy chip (-250)", ButtonSkin.Default, Anchor.AutoCenter);
			chipButton.Size = new Vector2(0.8f, 0.1f);
			chipButton.ButtonParagraph.Scale = 0.75f;
			panel.AddChild(chipButton);
		}
	}

	public override void Show() {
		if (chipButton != null) {
			chipButton.OnClick += OnChipBtnClick;
		}
		sellButton.OnClick += OnSellBtnClick;
		base.Show();
	}

	public override void Hide() {
		if (chipButton != null) {
			chipButton.OnClick -= OnChipBtnClick;
		}
		sellButton.OnClick -= OnSellBtnClick;
		base.Hide();
	}

	protected override void UpdateData() {
		sellButton.ButtonParagraph.Text = $"Sell (+{animal.Price})";

		dataParagraph.Text = $"Age: {animal.Age:0} days\n" +
			$"Sex: {animal.Gender}\n" +
			$"State: {(animal.Group != null ? animal.State : "Caught")}\n" +
			$"Hunger: {animal.HungerLevel:0} / 100\n" +
			$"Thirst: {animal.ThirstLevel:0} / 100\n" +
			$"Group Size: {animal.Group?.Size ?? 1} / {AnimalGroup.MAX_SIZE}\n" +
			$"Has Microchip: {(animal.HasChip ? "Yes" : "No")}";
		dataParagraph.Scale = SettingsMenu.Scale;
	}

	private void OnChipBtnClick(Entity e) {
		if (GameScene.Active.Model.Funds <= 250) {
			var alert = new AlertMenu("Can't buy chip", "You can't afford this.");
			alert.Show();
			return;
		}
		GameScene.Active.Model.Funds -= 250;
		this.animal.HasChip = true;
		panel.RemoveChild(chipButton);
		//chipParagraph = new Paragraph("This animal already has a microchip installed");
		//panel.AddChild(chipParagraph);
		chipButton = null;
	}

	private void OnSellBtnClick(Entity e) {
		if (GameScene.Active.Model.AnimalCount <= 5) {
			var alert = new AlertMenu("Can't sell animal", "There are too few animals in your safari.");
			alert.Show();
		} else {
			this.animal.Sell();
		}
	}
}
