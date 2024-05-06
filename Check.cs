﻿using AutomateToolSwap;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using StardewValley.Buildings;
using StardewValley.Monsters;
using Netcode;
using System.Threading;



public class Check
{
    private ModEntry ModEntry;
    private ModConfig config;
    public Check(ModEntry modEntry)
    {
        ModEntry = modEntry;
        config = ModEntry.Config;
    }

    public bool Objects(GameLocation location, Vector2 tile, Farmer player)
    {
        // Get the object at the specified tile
        StardewValley.Object obj = location.getObjectAtTile((int)tile.X, (int)tile.Y);
        bool itemCantBreak = !(player.CurrentItem is Pickaxe or Axe);
        // If obj is null, return false immediately
        if (obj == null)
            return false;

        switch (obj)
        {
            case var _ when obj.IsWeeds():
                if (config.AnyToolForWeeds && location is not MineShaft)
                    ModEntry.SetTool(player, typeof(Pickaxe), anyTool: true);
                else if (config.ScytheForWeeds)
                    ModEntry.SetTool(player, typeof(MeleeWeapon));
                return true;

            case var _ when obj.IsBreakableStone():
                if (config.PickaxeForStoneAndOres && (player.CurrentItem == null || !player.CurrentItem.Name.Contains("Bomb") && !player.CurrentItem.Name.Contains("Staircase")))
                    ModEntry.SetTool(player, typeof(Pickaxe));
                return true;

            case var _ when obj.IsTwig():
                if (config.AxeForTwigs)
                    ModEntry.SetTool(player, typeof(Axe));
                return true;

            case CrabPot crabPot when crabPot.bait.Value == null:
                if (config.BaitForCrabPot)
                    ModEntry.SetItem(player, "Bait", "Bait");
                return true;
        }

        switch (obj.Name)
        {
            case "Furnace":
                if (config.OresForFurnaces && itemCantBreak && (player.CurrentItem == null || !player.CurrentItem.Name.Contains("Ore")))
                    ModEntry.SetItem(player, "Resource", "Ore");
                return true;

            case "Cheese Press":
                if (config.MilkForCheesePress && itemCantBreak)
                    ModEntry.SetItem(player, "Animal Product", "Milk");
                return true;

            case "Mayonnaise Machine":
                if (config.EggsForMayoMachine && itemCantBreak)
                    ModEntry.SetItem(player, "Animal Product", "Egg");
                return true;

            case "Artifact Spot":
                if (config.HoeForArtifactSpots)
                    ModEntry.SetTool(player, typeof(Hoe));
                return true;

            case "Garden Pot":
                if (config.WateringCanForGardenPot && (player.CurrentItem == null || (itemCantBreak && player.CurrentItem.getCategoryName() != "Seed")))
                    ModEntry.SetTool(player, typeof(WateringCan));
                return true;

            case "Seed Spot":
                if (config.HoeForArtifactSpots)
                    ModEntry.SetTool(player, typeof(Hoe));
                return true;

            case "Barrel":
                if (config.WeaponForMineBarrels)
                    ModEntry.SetTool(player, typeof(MeleeWeapon), "Weapon");
                return true;

            case "Supply Crate":
                if (config.AnyToolForSupplyCrates)
                    ModEntry.SetTool(player, typeof(Hoe), anyTool: true);
                return true;

            case "Recycling Machine":
                if (config.TrashForRecycling && itemCantBreak)
                    ModEntry.SetItem(player, "Trash", "Joja");
                return true;

            case "Bone Mill":
                if (config.BoneForBoneMill && itemCantBreak)
                    ModEntry.SetItem(player, "Resource", "Bone Fragment");
                return true;

            case "Loom":
                if (config.WoolForLoom && itemCantBreak)
                    ModEntry.SetItem(player, "Animal Product", "Wool");
                return true;

            case "Fish Smoker":
                if (config.FishForSmoker && itemCantBreak)
                    ModEntry.SetItem(player, "Fish");
                return true;

            case "Bait Maker":
                if (config.FishForBaitMaker && itemCantBreak)
                    ModEntry.SetItem(player, "Fish");
                return true;

            case "Crystalarium":
                if (config.MineralsForCrystalarium && (player.CurrentItem == null || (itemCantBreak && player.CurrentItem.getCategoryName() != "Mineral")))
                    ModEntry.SetItem(player, "Mineral");
                return true;

            case "Seed Maker":
                if (config.SwapForSeedMaker && itemCantBreak)
                    ModEntry.SetItem(player, "Crops");
                return true;

            case "Keg":
                if (itemCantBreak && config.SwapForKegs != "None")
                    ModEntry.SetItem(player, "Crops", crops: config.SwapForKegs);
                return true;

            case "Preserves Jar":
                if (itemCantBreak && config.SwapForPreservesJar != "None")
                    ModEntry.SetItem(player, "Crops", crops: config.SwapForPreservesJar);
                return true;
        }
        return true;
    }

    public bool TerrainFeatures(GameLocation location, Vector2 tile, Farmer player)
    {
        foreach (var terrainFeature in location.largeTerrainFeatures)
        {
            if (!(terrainFeature is Bush) || !config.ScytheForBushes)
                break;

            var bush = terrainFeature as Bush;
            var bushBox = bush.getBoundingBox();
            var tilePixel = new Vector2(tile.X * Game1.tileSize, tile.Y * Game1.tileSize);

            if (bushBox.Contains((int)tilePixel.X, (int)tilePixel.Y) && bush.inBloom())
            {
                ModEntry.SetTool(player, typeof(MeleeWeapon));
                return true;
            }
        }

        if (!location.terrainFeatures.ContainsKey(tile))
            return false;

        var feature = location.terrainFeatures[tile];

        if (feature is Tree tree)
        {
            if (tree.hasMoss && tree.growthStage >= Tree.stageForMossGrowth && config.ScytheForMossOnTrees)
            {
                ModEntry.SetTool(player, typeof(MeleeWeapon));
                return true;
            }

            if (!config.AxeForTrees || (player.CurrentItem != null && player.CurrentItem.Name == "Tapper"))
                return true;

            if (!(tree.growthStage < Tree.treeStage && config.IgnoreGrowingTrees))
            {
                ModEntry.SetTool(player, typeof(Axe));
                return true;
            }

            return true;
        }

        if (feature is Grass && !(player.CurrentTool is MilkPail || player.CurrentTool is Shears) && config.ScytheForGrass)
        {
            ModEntry.SetTool(player, typeof(MeleeWeapon));
            return true;
        }

        if (feature is HoeDirt hoeDirt)
        {
            if (hoeDirt.crop == null && config.SeedForTilledDirt)
            {
                if (!(config.PickaxeOverWateringCan && player.CurrentTool is Pickaxe))
                    if (player.CurrentItem == null || player.CurrentItem.getCategoryName() != "Seed" || player.CurrentItem.HasContextTag("tree_seed_item"))
                        ModEntry.SetItem(player, "Seed");

                return true;
            }

            if (hoeDirt.crop != null && (hoeDirt.readyForHarvest() || hoeDirt.crop.dead) && config.ScytheForCrops)
            {
                ModEntry.SetTool(player, typeof(MeleeWeapon));
                return true;
            }

            if (hoeDirt.crop != null && !hoeDirt.HasFertilizer() && (player.CurrentItem == null || (player.CurrentItem.getCategoryName() != "Fertilizer" && config.FertilizerForCrops)))
            {
                if (!(config.PickaxeOverWateringCan && player.CurrentTool is Pickaxe))
                    ModEntry.SetItem(player, "Fertilizer", "Tree");

            }

            if (hoeDirt.crop != null && hoeDirt.crop.whichForageCrop == "2" && config.HoeForGingerCrop)
            {
                ModEntry.SetTool(player, typeof(Hoe));
                return true;
            }

            if (hoeDirt.crop != null && !hoeDirt.isWatered() && !hoeDirt.readyForHarvest() && (player.CurrentItem == null || (player.CurrentItem.getCategoryName() != "Fertilizer" && config.WateringCanForUnwateredCrop && !(player.isRidingHorse() && player.mount.Name.Contains("tractor") && player.CurrentTool is Hoe))))
            {
                if (!(config.PickaxeOverWateringCan && player.CurrentTool is Pickaxe))
                {
                    ModEntry.SetTool(player, typeof(WateringCan));
                    return true;
                }
            }

            return true;
        }

        return false;
    }

    public bool ResourceClumps(GameLocation location, Vector2 tilePosition, Farmer farmer)
    {
        bool IsStumpOrLog(ResourceClump resourceClump)
        {
            return new List<int> { 602, 600 }.Contains(resourceClump.parentSheetIndex);
        }

        bool IsBoulder(ResourceClump resourceClump)
        {
            return new List<int> { 758, 756, 754, 752, 672, 622, 148 }.Contains(resourceClump.parentSheetIndex);
        }

        foreach (var resourceClump in location.resourceClumps)
        {
            if (resourceClump.occupiesTile((int)tilePosition.X, (int)tilePosition.Y))
            {
                if (config.AxeForGiantCrops && resourceClump is GiantCrop)
                {
                    ModEntry.SetTool(farmer, typeof(Axe));
                    return true;
                }

                if (config.AxeForStumpsAndLogs && IsStumpOrLog(resourceClump))
                {
                    ModEntry.SetTool(farmer, typeof(Axe));
                    return true;
                }

                if (config.PickaxeForBoulders && IsBoulder(resourceClump))
                {
                    ModEntry.SetTool(farmer, typeof(Pickaxe));
                    return true;
                }
            }
        }

        return false;
    }

    public bool Monsters(GameLocation location, Vector2 tile, Farmer player)
    {
        foreach (var character in location.characters)
        {
            if (character.IsMonster && Vector2.Distance(tile, character.Tile) < config.MonsterRangeDetection)
            {
                if (character is RockCrab crab)
                {
                    if (config.IgnoreCrabs)
                        return true;

                    var isShellLess = ModEntry.Helper.Reflection.GetField<NetBool>(crab, "shellGone").GetValue();
                    if (!isShellLess && !crab.isMoving())
                    {
                        ModEntry.SetTool(player, typeof(Pickaxe));
                        return true;
                    }
                }

                if (player.CurrentItem == null || !player.CurrentItem.Name.Contains("Bomb") && !player.CurrentItem.Name.Contains("Staircase"))
                {
                    ModEntry.SetTool(player, typeof(MeleeWeapon), "Weapon");
                    return true;
                }

                return true;
            }
        }

        return false;
    }

    public bool Water(GameLocation location, Vector2 tile, Farmer player)
    {
        if (IsPetBowlOrStable(location, tile) && config.WateringCanForPetBowl)
        {
            ModEntry.SetTool(player, typeof(WateringCan));
            return true;
        }

        if (IsPanSpot(location, tile, player) && config.PanForPanningSpots)
        {
            ModEntry.SetTool(player, typeof(Pan));
            return true;
        }

        if (IsWater(location, tile, player) && config.FishingRodOnWater && !(location is Farm || location is VolcanoDungeon || location.InIslandContext() || location.isGreenhouse))
        {
            ModEntry.SetTool(player, typeof(FishingRod));
            return true;
        }

        if ((IsWaterSource(location, tile) || IsWater(location, tile, player)) && config.WateringCanForWater)
        {
            ModEntry.SetTool(player, typeof(WateringCan));
            return true;
        }

        bool IsPetBowlOrStable(GameLocation location, Vector2 tile)
        {
            var building = location.getBuildingAt(tile);
            return building != null && (building.GetType() == typeof(PetBowl) || building.GetType() == typeof(Stable));
        }
        bool IsPanSpot(GameLocation location, Vector2 tile, Farmer player)
        {
            var toolLocation = player.GetToolLocation(false) / 64;
            var orePanRect = new Rectangle(player.currentLocation.orePanPoint.X * 64 - 64, player.currentLocation.orePanPoint.Y * 64 - 64, 256, 256);
            return orePanRect.Contains((int)tile.X * 64, (int)tile.Y * 64) && Utility.distance((float)player.StandingPixel.X, (float)orePanRect.Center.X, (float)player.StandingPixel.Y, (float)orePanRect.Center.Y) <= 192f;
        }
        bool IsWater(GameLocation location, Vector2 tile, Farmer player)
        {
            return location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") != null && !(player.CurrentTool is WateringCan || player.CurrentTool is Pan);
        }
        bool IsWaterSource(GameLocation location, Vector2 tile)
        {
            return location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "WaterSource", "Back") != null;
        }

        return false;
    }

    public bool Animals(GameLocation location, Vector2 tile, Farmer player)
    {
        // Check for animals to interact with
        if (!(location is Farm or AnimalHouse))
            return false;

        string[] animalsThatCanBeMilked = { "Goat", "Cow" };
        string[] animalsThatCanBeSheared = { "Sheep" };

        foreach (FarmAnimal animal in location.getAllFarmAnimals())
        {
            float distanceToAnimal = Vector2.Distance(tile, animal.Tile);

            if (config.MilkPailForCowsAndGoats && animalsThatCanBeMilked.Any(animal.displayType.Contains)
                && distanceToAnimal <= 1 && animal.currentLocation == player.currentLocation)
            {
                ModEntry.SetTool(player, typeof(MilkPail));
                return true;
            }

            if (config.ShearsForSheeps && animalsThatCanBeSheared.Any(animal.displayType.Contains)
                && distanceToAnimal <= 1 && animal.currentLocation == player.currentLocation)
            {
                ModEntry.SetTool(player, typeof(Shears));
                return true;
            }
        }
        // Check for feeding bench availability
        bool isFeedingBench = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Trough", "Back") != null;
        if (location is AnimalHouse && isFeedingBench)
        {
            ModEntry.SetItem(player, "", "Hay");
            return true;
        }

        return false;
    }

    public bool DiggableSoil(GameLocation location, Vector2 tile, Farmer player)
    {
        if (!ModEntry.isTractorModInstalled || (player.isRidingHorse() && player.mount.Name.Contains("tractor")))
            return false;

        bool isNotScythe = player.CurrentItem?.getCategoryName().Contains("Level") != true;
        bool isDiggable = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
        bool isFightingLocations = location is Mine or MineShaft or VolcanoDungeon;

        if (!config.HoeForDiggableSoil || !isDiggable || isFightingLocations || location.isPath(tile))
            return false;
        if (player.CurrentItem is MeleeWeapon && isNotScythe && Game1.spawnMonstersAtNight)
            return false;
        if (player.CurrentItem is FishingRod or GenericTool or Wand)
            return false;

        if (player.CurrentItem == null || !player.CurrentItem.canBePlacedHere(location, tile, CollisionMask.All, true))
        {
            ModEntry.SetTool(player, typeof(Hoe));
            return true;
        }

        return false;
    }
}
