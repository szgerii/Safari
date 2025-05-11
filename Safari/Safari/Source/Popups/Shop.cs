using Engine.Input;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Helpers;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;

namespace Safari.Popups;

public class Shop : PopupMenu, IResettableSingleton {
    public const int TREE_COST = 400;
    public const int GRASS_COST = 25;
    public const int ROAD_COST = 15;
    public const int WATER_COST = 30;
    public const int BUSH_COST = 65;
    public const int JEEP_COST = 2000;

    private static Shop instance;
    public static Shop Instance {
        get {
            instance ??= new();
            return instance;
        }
    }

    public static ConstructionHelperCmp CHelper => GameScene.Active.Model.Level.ConstructionHelperCmp;

    public static void ResetSingleton() {
        instance?.Hide();
        instance = null;
    }

    public void BuyAnimal(AnimalSpecies type, Gender gender) {
        if (GameScene.Active.Model.Funds <= type.GetPrice()) {
            new AlertMenu("Funds", $"You don't have enough money to buy this {type.GetDisplayName()}").Show();
            return;
        }
        Type animalType = type.GetAnimalType();
        object[] constructorArgs = { MapBuilder.GetRandomSpawn(GameScene.Active.Model.Level, type.GetSize()), gender };
        Animal temp = (Animal)Activator.CreateInstance(animalType, constructorArgs);
        Game.AddObject(temp);
        GameScene.Active.Model.Funds -= type.GetPrice();
    }

    private Shop() {

    }
}
