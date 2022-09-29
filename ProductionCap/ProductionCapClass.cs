using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace ProductionCap
{
    public class ProductionCapClass : MelonMod
    {
        private System.Collections.Generic.List<ResourceCap> listResourceCap = new System.Collections.Generic.List<ResourceCap>();

        public override void OnUpdate()
        {
            GameObject go = GameObject.FindGameObjectWithTag("GameManager");
            if (go != null)
            {
                GameManager gameManager = go.GetComponent<GameManager>();

                if (gameManager != null)
                {
                    foreach(ResourceCap resourceCap in listResourceCap)
                    {
                        this.updateBuildingProduction(gameManager, resourceCap);
                    }
                }
            }

            base.OnUpdate();
        }

        public override void OnApplicationStart()
        {
            string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            string filePathRelativeToAssembly = location + "\\Farthest Frontier\\Mods\\ProductionCap.xml";

            XmlDocument doc = new XmlDocument();
            doc.Load(filePathRelativeToAssembly);

            XmlNode buildings = doc.DocumentElement.SelectSingleNode("/ProductionCap/Buildings");

            foreach (XmlNode building in buildings.ChildNodes)
            {
                string buildingName = building.SelectSingleNode("Name").InnerText;
                XmlNode resources = building.SelectSingleNode("Resources");
                foreach (XmlNode resource in resources.ChildNodes)
                {
                    string name = resource.SelectSingleNode("Name").InnerText;
                    int stopAt = int.MaxValue;
                    int resumeAt = 0;
                    try
                    {
                        stopAt = Int32.Parse(resource.SelectSingleNode("StopAt").InnerText);
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Error("Error while parsing xml file for " + name + " > StopAt: " + e.Message);
                    }
                    try
                    {
                        resumeAt = Int32.Parse(resource.SelectSingleNode("ResumeAt").InnerText);
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Error("Error while parsing xml file for " + name + " > ResumeAt: " + e.Message);
                    }
                    ResourceCap resourceCap = new ResourceCap(name, stopAt, resumeAt);
                    MelonLogger.Msg("added resource cap : " + name + ", stopAt: " + stopAt + ", resumeAt: " + resumeAt);
                    this.listResourceCap.Add(resourceCap);
                }
            }

            base.OnApplicationStart();
        }

        private bool isAllProductionDisabled(EnterableBuilding building)
        {
            int manufactureDefinitionCount = building.manufactureDefinitions.Count;
            bool result = true;
            for (int i = 0; i < manufactureDefinitionCount; i++)
            {
                result = result && (building.GetBatchSizeByManufactureDefinition(building.manufactureDefinitions[i]) == 0);
            }
            return result;
        }

        private void updateBuildingProduction(GameManager gameManager, ResourceCap resourceCap)
        {
            if (gameManager == null)
            {
                return;
            }

            switch (resourceCap.Name.ToLower().TrimStart().TrimEnd())
            {
                //amenities
                case "medicine":
                    List<ApothecaryShop> apothecaryShops = gameManager.resourceManager.apothecaryShops;
                    if (gameManager.resourceManager.medicineItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (ApothecaryShop apothecaryShop in apothecaryShops)
                        {
                            apothecaryShop.SetWorkEnabled(false, true);
                        }
                    } 
                    else if (gameManager.resourceManager.medicineItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (ApothecaryShop apothecaryShop in apothecaryShops)
                        {
                            apothecaryShop.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                //storage
                case "barrel":
                    List<CooperBuilding> cooperBuildings = gameManager.resourceManager.cooperBuildings;
                    if (gameManager.resourceManager.barrelItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (CooperBuilding cooperBuilding in cooperBuildings)
                        {
                            cooperBuilding.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.barrelItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (CooperBuilding cooperBuilding in cooperBuildings)
                        {
                            cooperBuilding.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                //food
                case "fish":
                    List<FishingShack> fishingShacks = gameManager.resourceManager.fishingShacks;
                    if (gameManager.resourceManager.fishItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (FishingShack fishingShack in fishingShacks)
                        {
                            fishingShack.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.fishItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (FishingShack fishingShack in fishingShacks)
                        {
                            fishingShack.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "smoked fish":
                    if (gameManager.resourceManager.smokedFishItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<SmokeHouse> smokeHouses = gameManager.resourceManager.smokeHouses;
                        foreach (SmokeHouse smokeHouse in smokeHouses)
                        {
                            smokeHouse.SetManufactureDefinitionBatchSize(smokeHouse.manufactureDefinitions[1], 0);
                            if (isAllProductionDisabled(smokeHouse))
                            {
                                smokeHouse.SetWorkEnabled(false, true);
                            }

                        }
                    }
                    else if (gameManager.resourceManager.smokedFishItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<SmokeHouse> smokeHouses = gameManager.resourceManager.smokeHouses;
                        foreach (SmokeHouse smokeHouse in smokeHouses)
                        {
                            int batchCount = smokeHouse.GetBatchSizeByManufactureDefinition(smokeHouse.manufactureDefinitions[1]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            smokeHouse.SetManufactureDefinitionBatchSize(smokeHouse.manufactureDefinitions[1], targetCount);
                            smokeHouse.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "smoked meat":
                    if (gameManager.resourceManager.smokedMeatItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<SmokeHouse> smokeHouses = gameManager.resourceManager.smokeHouses;
                        foreach (SmokeHouse smokeHouse in smokeHouses)
                        {
                            smokeHouse.SetManufactureDefinitionBatchSize(smokeHouse.manufactureDefinitions[0], 0);
                            if (isAllProductionDisabled(smokeHouse))
                            {
                                smokeHouse.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.smokedFishItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<SmokeHouse> smokeHouses = gameManager.resourceManager.smokeHouses;
                        foreach (SmokeHouse smokeHouse in smokeHouses)
                        {
                            int batchCount = smokeHouse.GetBatchSizeByManufactureDefinition(smokeHouse.manufactureDefinitions[0]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            smokeHouse.SetManufactureDefinitionBatchSize(smokeHouse.manufactureDefinitions[0], targetCount);
                            smokeHouse.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "flour":
                    List<Windmill> windmills = gameManager.resourceManager.windmills;
                    if (gameManager.resourceManager.flourItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Windmill windmill in windmills)
                        {
                            windmill.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.flourItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Windmill windmill in windmills)
                        {
                            windmill.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "bread":
                    List<Bakery> bakeries = gameManager.resourceManager.bakeries;
                    if (gameManager.resourceManager.breadItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Bakery bakery in bakeries)
                        {
                            bakery.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.breadItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Bakery bakery in bakeries)
                        {
                            bakery.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "cheese":
                    List<Cheesemaker> cheesemakers = gameManager.resourceManager.cheesemakers;
                    if (gameManager.resourceManager.cheeseItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Cheesemaker cheesemaker in cheesemakers)
                        {
                            cheesemaker.SetWorkEnabled(false, true);
                        }
                    } 
                    else if (gameManager.resourceManager.cheeseItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Cheesemaker cheesemaker in cheesemakers)
                        {
                            cheesemaker.SetWorkEnabled(false, true);
                        }
                    }
                    break;
                //resources
                case "firewood":
                    List<WoodCutterBuilding> woodCutterBuildings = gameManager.resourceManager.woodCutterBuildings;
                    if (gameManager.resourceManager.firewoodItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (WoodCutterBuilding woodCutterBuilding in woodCutterBuildings)
                        {
                            woodCutterBuilding.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.firewoodItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (WoodCutterBuilding woodCutterBuilding in woodCutterBuildings)
                        {
                            woodCutterBuilding.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "plank":
                    List<SawPitBuilding> sawPitBuildings = gameManager.resourceManager.sawPitBuildings;
                    if (gameManager.resourceManager.plankItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (SawPitBuilding sawPitBuilding in sawPitBuildings)
                        {
                            sawPitBuilding.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.plankItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (SawPitBuilding sawPitBuilding in sawPitBuildings)
                        {
                            sawPitBuilding.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "hide coat":
                    List<Tannery> tanneries = gameManager.resourceManager.tanneries;
                    if (gameManager.resourceManager.hideCoatItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Tannery tannery in tanneries)
                        {
                            tannery.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.hideCoatItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Tannery tannery in tanneries)
                        {
                            tannery.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "shoes":
                    List<CobblerShop> cobblerShops = gameManager.resourceManager.cobblerShops;
                    if (gameManager.resourceManager.shoesItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (CobblerShop cobblerShop in cobblerShops)
                        {
                            cobblerShop.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.shoesItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (CobblerShop cobblerShop in cobblerShops)
                        {
                            cobblerShop.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "arrows":
                    if (gameManager.resourceManager.arrowItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<FletcherBuilding> fletcherBuildings = gameManager.resourceManager.fletcherBuildings;
                        foreach (FletcherBuilding fletcherBuilding in fletcherBuildings)
                        {
                            fletcherBuilding.SetManufactureDefinitionBatchSize(fletcherBuilding.manufactureDefinitions[0], 0);
                            if (isAllProductionDisabled(fletcherBuilding))
                            {
                                fletcherBuilding.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.arrowItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<FletcherBuilding> fletcherBuildings = gameManager.resourceManager.fletcherBuildings;
                        foreach (FletcherBuilding fletcherBuilding in fletcherBuildings)
                        {
                            int batchCount = fletcherBuilding.GetBatchSizeByManufactureDefinition(fletcherBuilding.manufactureDefinitions[0]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            fletcherBuilding.SetManufactureDefinitionBatchSize(fletcherBuilding.manufactureDefinitions[0], targetCount);
                            fletcherBuilding.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "bow":
                    if (gameManager.resourceManager.bowItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<FletcherBuilding> fletcherBuildings = gameManager.resourceManager.fletcherBuildings;
                        foreach (FletcherBuilding fletcherBuilding in fletcherBuildings)
                        {
                            fletcherBuilding.SetManufactureDefinitionBatchSize(fletcherBuilding.manufactureDefinitions[1], 0);
                            if (isAllProductionDisabled(fletcherBuilding))
                            {
                                fletcherBuilding.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.bowItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<FletcherBuilding> fletcherBuildings = gameManager.resourceManager.fletcherBuildings;
                        foreach (FletcherBuilding fletcherBuilding in fletcherBuildings)
                        {
                            int batchCount = fletcherBuilding.GetBatchSizeByManufactureDefinition(fletcherBuilding.manufactureDefinitions[1]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            fletcherBuilding.SetManufactureDefinitionBatchSize(fletcherBuilding.manufactureDefinitions[1], targetCount);
                            fletcherBuilding.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "crossbow":
                    List<FletcherBuilding> fletcherBuildings1 = gameManager.resourceManager.fletcherBuildings;
                    foreach (FletcherBuilding fletcherBuilding in fletcherBuildings1)
                    {
                        if (fletcherBuilding.manufactureDefinitions.Count == 3)
                        {
                            if (gameManager.resourceManager.crossbowItemInfo.unusedCount > resourceCap.StopAt)
                            {
                                fletcherBuilding.SetManufactureDefinitionBatchSize(fletcherBuilding.manufactureDefinitions[2], 0);
                                if (isAllProductionDisabled(fletcherBuilding))
                                {
                                    fletcherBuilding.SetWorkEnabled(false, true);
                                }
                            }
                            else if (gameManager.resourceManager.crossbowItemInfo.unusedCount < resourceCap.ResumeAt)
                            {
                                int batchCount = fletcherBuilding.GetBatchSizeByManufactureDefinition(fletcherBuilding.manufactureDefinitions[2]);
                                int targetCount = batchCount == 0 ? 1 : batchCount;
                                fletcherBuilding.SetManufactureDefinitionBatchSize(fletcherBuilding.manufactureDefinitions[2], targetCount);
                                fletcherBuilding.SetWorkEnabled(true, true);
                            }
                        }
                    }  
                    break;
                case "basket":
                    List<BasketShop> basketShops = gameManager.resourceManager.basketShops;
                    if (gameManager.resourceManager.basketItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (BasketShop basketShop in basketShops)
                        {
                            basketShop.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.basketItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (BasketShop basketShop in basketShops)
                        {
                            basketShop.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "clay":
                    List<Mine> clayMines = gameManager.resourceManager.mines;
                    if (gameManager.resourceManager.clayItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Mine mine in clayMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach(Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.clayItemInfo.item))
                                {
                                    mine.SetWorkEnabled(false, true);
                                }
                            }
                        }
                    }
                    else if (gameManager.resourceManager.clayItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Mine mine in clayMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach (Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.clayItemInfo.item))
                                {
                                    mine.SetWorkEnabled(true, true);
                                }
                            }
                        }
                    }
                    break;
                case "iron ore":
                    List<Mine> IronMines = gameManager.resourceManager.mines;
                    if (gameManager.resourceManager.ironOreItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Mine mine in IronMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach(Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.ironOreItemInfo.item))
                                {
                                    mine.SetWorkEnabled(false, true);
                                }
                            }
                        }
                    }
                    else if (gameManager.resourceManager.ironOreItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Mine mine in IronMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach (Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.ironOreItemInfo.item))
                                {
                                    mine.SetWorkEnabled(true, true);
                                }
                            }
                        }
                    }
                    break;
                case "gold ore":
                    List<Mine> goldMines = gameManager.resourceManager.mines;
                    if (gameManager.resourceManager.goldOreItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Mine mine in goldMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach(Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.goldOreItemInfo.item))
                                {
                                    mine.SetWorkEnabled(false, true);
                                }
                            }
                        }
                    }
                    else if (gameManager.resourceManager.goldOreItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Mine mine in goldMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach (Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.goldOreItemInfo.item))
                                {
                                    mine.SetWorkEnabled(true, true);
                                }
                            }
                        }
                    }
                    break;
                case "coal":
                    List<Mine> CoalMines = gameManager.resourceManager.mines;
                    List<CharcoalKiln> charcoalKilns = gameManager.resourceManager.charcoalKilns;
                    if (gameManager.resourceManager.coalItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Mine mine in CoalMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach(Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.coalItemInfo.item))
                                {
                                    mine.SetWorkEnabled(false, true);
                                }
                            }
                        }                    
                        foreach (CharcoalKiln charcoalKiln in charcoalKilns)
                        {
                            charcoalKiln.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.coalItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Mine mine in CoalMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach (Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.coalItemInfo.item))
                                {
                                    mine.SetWorkEnabled(true, true);
                                }
                            }
                        }
                        foreach (CharcoalKiln charcoalKiln in charcoalKilns)
                        {
                            charcoalKiln.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "sand":
                    List<Mine> sandMines = gameManager.resourceManager.mines;
                    if (gameManager.resourceManager.sandItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Mine mine in sandMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach(Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.sandItemInfo.item))
                                {
                                    mine.SetWorkEnabled(false, true);
                                }
                            }
                        }
                    }
                    else if (gameManager.resourceManager.sandItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Mine mine in sandMines)
                        {
                            List<Item> items = mine.GetProducedManufacturingItems();
                            foreach (Item item in items)
                            {
                                if (item.Equals(gameManager.resourceManager.sandItemInfo.item))
                                {
                                    mine.SetWorkEnabled(true, true);
                                }
                            }
                        }
                    }
                    break;
                case "pottery":
                    List<PotterBuilding> potterBuildings = gameManager.resourceManager.potterBuildings;
                    if (gameManager.resourceManager.potteryItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (PotterBuilding potterBuilding in potterBuildings)
                        {
                            potterBuilding.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.potteryItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (PotterBuilding potterBuilding in potterBuildings)
                        {
                            potterBuilding.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "candle":
                    List<CandleShop> candleShops = gameManager.resourceManager.candleShops;
                    if (gameManager.resourceManager.candleItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (CandleShop candleShop in candleShops)
                        {
                            candleShop.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.candleItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (CandleShop candleShop in candleShops)
                        {
                            candleShop.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "soap":
                    List<SoapShop> soapShops = gameManager.resourceManager.soapShops;
                    if (gameManager.resourceManager.soapItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (SoapShop soapShop in soapShops)
                        {
                            soapShop.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.soapItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (SoapShop soapShop in soapShops)
                        {
                            soapShop.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "shields":
                    if (gameManager.resourceManager.shieldItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<Armory> armories = gameManager.resourceManager.armories;
                        foreach (Armory armory in armories)
                        {
                            armory.SetManufactureDefinitionBatchSize(armory.manufactureDefinitions[0], 0);
                            if (isAllProductionDisabled(armory))
                            {
                                armory.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.shieldItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<Armory> armories = gameManager.resourceManager.armories;
                        foreach (Armory armory in armories)
                        {
                            int batchCount = armory.GetBatchSizeByManufactureDefinition(armory.manufactureDefinitions[0]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            armory.SetManufactureDefinitionBatchSize(armory.manufactureDefinitions[0], targetCount);
                            armory.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "hauberks":
                    if (gameManager.resourceManager.hauberkItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<Armory> armories = gameManager.resourceManager.armories;
                        foreach (Armory armory in armories)
                        {
                            armory.SetManufactureDefinitionBatchSize(armory.manufactureDefinitions[1], 0);
                            if (isAllProductionDisabled(armory))
                            {
                                armory.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.hauberkItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<Armory> armories = gameManager.resourceManager.armories;
                        foreach (Armory armory in armories)
                        {
                            int batchCount = armory.GetBatchSizeByManufactureDefinition(armory.manufactureDefinitions[1]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            armory.SetManufactureDefinitionBatchSize(armory.manufactureDefinitions[1], targetCount);
                            armory.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "platemail armor":
                    List<Armory> armories1 = gameManager.resourceManager.armories;
                    foreach (Armory armory in armories1)
                    {
                        if (armory.manufactureDefinitions.Count == 3)
                        {
                            if (gameManager.resourceManager.platemailItemInfo.unusedCount > resourceCap.StopAt)
                            {
                                armory.SetManufactureDefinitionBatchSize(armory.manufactureDefinitions[2], 0);
                                if (isAllProductionDisabled(armory))
                                {
                                    armory.SetWorkEnabled(false, true);
                                }
                            }
                            else if (gameManager.resourceManager.platemailItemInfo.unusedCount < resourceCap.ResumeAt)
                            {
                                int batchCount = armory.GetBatchSizeByManufactureDefinition(armory.manufactureDefinitions[2]);
                                int targetCount = batchCount == 0 ? 1 : batchCount;
                                armory.SetManufactureDefinitionBatchSize(armory.manufactureDefinitions[2], targetCount);
                                armory.SetWorkEnabled(true, true);
                            }
                        }
                    }
                    break;
                case "furniture":
                    List<FurnitureWorkshop> furnitureWorkshops = gameManager.resourceManager.furnitureWorkshops;
                    if (gameManager.resourceManager.furnitureItemInfo.unusedCount > resourceCap.StopAt)
                    { 
                        foreach (FurnitureWorkshop furnitureWorkshop in furnitureWorkshops)
                        {
                            furnitureWorkshop.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.furnitureItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (FurnitureWorkshop furnitureWorkshop in furnitureWorkshops)
                        {
                            furnitureWorkshop.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "beer":
                    List<Brewery> breweries = gameManager.resourceManager.breweries;
                    if (gameManager.resourceManager.wheatBeerItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Brewery brewery in breweries)
                        {
                            brewery.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.wheatBeerItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Brewery brewery in breweries)
                        {
                            brewery.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "iron ingot":
                    if (gameManager.resourceManager.ironItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<Foundry> foundries = gameManager.resourceManager.foundries;
                        foreach (Foundry foundry in foundries)
                        {
                            foundry.SetManufactureDefinitionBatchSize(foundry.manufactureDefinitions[0], 0);
                            if (isAllProductionDisabled(foundry))
                            {
                                foundry.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.ironItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<Foundry> foundries = gameManager.resourceManager.foundries;
                        foreach (Foundry foundry in foundries)
                        {
                            int batchCount = foundry.GetBatchSizeByManufactureDefinition(foundry.manufactureDefinitions[0]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            foundry.SetManufactureDefinitionBatchSize(foundry.manufactureDefinitions[0], targetCount);
                            foundry.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "gold ingot":
                    if (gameManager.resourceManager.goldIngotItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<Foundry> foundries = gameManager.resourceManager.foundries;
                        foreach (Foundry foundry in foundries)
                        {
                            foundry.SetManufactureDefinitionBatchSize(foundry.manufactureDefinitions[1], 0);
                            if (isAllProductionDisabled(foundry))
                            {
                                foundry.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.goldIngotItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<Foundry> foundries = gameManager.resourceManager.foundries;
                        foreach (Foundry foundry in foundries)
                        {
                            int batchCount = foundry.GetBatchSizeByManufactureDefinition(foundry.manufactureDefinitions[1]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            foundry.SetManufactureDefinitionBatchSize(foundry.manufactureDefinitions[1], targetCount);
                        }
                    }
                    break;
                case "tools":
                    if (gameManager.resourceManager.toolItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<BlacksmithForge> blacksmithForges = gameManager.resourceManager.blacksmithForges;
                        foreach (BlacksmithForge blacksmithForge in blacksmithForges)
                        {
                            blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[0], 0);
                            if (isAllProductionDisabled(blacksmithForge))
                            {
                                blacksmithForge.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.toolItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<BlacksmithForge> blacksmithForges = gameManager.resourceManager.blacksmithForges;
                        foreach (BlacksmithForge blacksmithForge in blacksmithForges)
                        {
                            int batchCount = blacksmithForge.GetBatchSizeByManufactureDefinition(blacksmithForge.manufactureDefinitions[0]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[0], targetCount);
                            blacksmithForge.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "heavy tools":
                    if (gameManager.resourceManager.heavyToolItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<BlacksmithForge> blacksmithForges = gameManager.resourceManager.blacksmithForges;
                        foreach (BlacksmithForge blacksmithForge in blacksmithForges)
                        {
                            blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[2], 0);
                            if (isAllProductionDisabled(blacksmithForge))
                            {
                                blacksmithForge.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.heavyToolItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<BlacksmithForge> blacksmithForges = gameManager.resourceManager.blacksmithForges;
                        foreach (BlacksmithForge blacksmithForge in blacksmithForges)
                        {
                            int batchCount = blacksmithForge.GetBatchSizeByManufactureDefinition(blacksmithForge.manufactureDefinitions[2]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[2], targetCount);
                            blacksmithForge.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "weapons":
                    if (gameManager.resourceManager.weaponItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        List<BlacksmithForge> blacksmithForges = gameManager.resourceManager.blacksmithForges;
                        foreach (BlacksmithForge blacksmithForge in blacksmithForges)
                        {
                            blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[1], 0);
                            if (isAllProductionDisabled(blacksmithForge))
                            {
                                blacksmithForge.SetWorkEnabled(false, true);
                            }
                        }
                    }
                    else if (gameManager.resourceManager.weaponItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        List<BlacksmithForge> blacksmithForges = gameManager.resourceManager.blacksmithForges;
                        foreach (BlacksmithForge blacksmithForge in blacksmithForges)
                        {
                            int batchCount = blacksmithForge.GetBatchSizeByManufactureDefinition(blacksmithForge.manufactureDefinitions[1]);
                            int targetCount = batchCount == 0 ? 1 : batchCount;
                            blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[1], targetCount);
                            blacksmithForge.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "heavy weapons":
                    List<BlacksmithForge> blacksmithForges1 = gameManager.resourceManager.blacksmithForges;
                    foreach (BlacksmithForge blacksmithForge in blacksmithForges1)
                    {
                        if (blacksmithForge.manufactureDefinitions.Count == 4)
                        {
                            if (gameManager.resourceManager.heavyWeaponItemInfo.unusedCount > resourceCap.StopAt)
                            {
                                blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[3], 0);
                                if (isAllProductionDisabled(blacksmithForge))
                                {
                                    blacksmithForge.SetWorkEnabled(false, true);
                                }
                            }
                            else if (gameManager.resourceManager.heavyWeaponItemInfo.unusedCount < resourceCap.ResumeAt)
                            {
                                int batchCount = blacksmithForge.GetBatchSizeByManufactureDefinition(blacksmithForge.manufactureDefinitions[3]);
                                int targetCount = batchCount == 0 ? 1 : batchCount;
                                blacksmithForge.SetManufactureDefinitionBatchSize(blacksmithForge.manufactureDefinitions[3], targetCount);
                                blacksmithForge.SetWorkEnabled(true, true);
                            }
                        }
                    }
                    break;
                case "brick":
                    List<Brickyard> brickyards = gameManager.resourceManager.brickyards;
                    if (gameManager.resourceManager.brickItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Brickyard brickyard in brickyards)
                        {
                            brickyard.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.brickItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Brickyard brickyard in brickyards)
                        {
                            brickyard.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                case "glass":
                    List<Glassmaker> glassmakers = gameManager.resourceManager.glassmakers;
                    if (gameManager.resourceManager.glassItemInfo.unusedCount > resourceCap.StopAt)
                    {
                        foreach (Glassmaker glassmaker in glassmakers)
                        {
                            glassmaker.SetWorkEnabled(false, true);
                        }
                    }
                    else if (gameManager.resourceManager.glassItemInfo.unusedCount < resourceCap.ResumeAt)
                    {
                        foreach (Glassmaker glassmaker in glassmakers)
                        {
                            glassmaker.SetWorkEnabled(true, true);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
