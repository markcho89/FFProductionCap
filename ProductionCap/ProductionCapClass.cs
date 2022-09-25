using System;
using System.Collections.Generic;
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

            XmlNode resources = doc.DocumentElement.SelectSingleNode("/ProductionCap/Resources");

            foreach (XmlNode resource in resources.ChildNodes)
            {
                string name = resource.SelectSingleNode("Name").InnerText;
                string value = resource.SelectSingleNode("Amount").InnerText;

                ResourceCap resourceCap = new ResourceCap(name, Int32.Parse(value));
                this.listResourceCap.Add(resourceCap);
                MelonLogger.Msg("Successfully parsed XML: Name = " + name + ", Amount = " + value);
            }

            base.OnApplicationStart();
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
                case "apothecary shop":
                    Il2CppSystem.Collections.Generic.List<ApothecaryShop> apothecaryShops = gameManager.resourceManager.apothecaryShops;
                    foreach (ApothecaryShop apothecaryShop in apothecaryShops)
                    {
                        apothecaryShop.SetWorkEnabled(gameManager.resourceManager.medicineItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                //storage
                case "cooper":
                    Il2CppSystem.Collections.Generic.List<CooperBuilding> cooperBuildings = gameManager.resourceManager.cooperBuildings;
                    foreach (CooperBuilding cooperBuilding in cooperBuildings)
                    {
                        cooperBuilding.SetWorkEnabled(gameManager.resourceManager.barrelItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                //food
                case "fishing shack":
                    Il2CppSystem.Collections.Generic.List<FishingShack> fishingShacks = gameManager.resourceManager.fishingShacks;
                    foreach (FishingShack fishingShack in fishingShacks)
                    {
                        fishingShack.SetWorkEnabled(gameManager.resourceManager.fishItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "windmill":
                    Il2CppSystem.Collections.Generic.List<Windmill> windmills = gameManager.resourceManager.windmills;
                    foreach (Windmill windmill in windmills)
                    {
                        windmill.SetWorkEnabled(gameManager.resourceManager.flourItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "bakery":
                    Il2CppSystem.Collections.Generic.List<Bakery> bakeries = gameManager.resourceManager.bakeries;
                    foreach (Bakery bakery in bakeries)
                    {
                        bakery.SetWorkEnabled(gameManager.resourceManager.breadItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "cheesemaker":
                    Il2CppSystem.Collections.Generic.List<Cheesemaker> cheesemakers = gameManager.resourceManager.cheesemakers;
                    foreach (Cheesemaker cheesemaker in cheesemakers)
                    {
                        cheesemaker.SetWorkEnabled(gameManager.resourceManager.cheeseItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                //resources
                case "firewood splitter":
                    Il2CppSystem.Collections.Generic.List<WoodCutterBuilding> woodCutterBuildings = gameManager.resourceManager.woodCutterBuildings;
                    foreach (WoodCutterBuilding woodCutterBuilding in woodCutterBuildings)
                    {
                        woodCutterBuilding.SetWorkEnabled(gameManager.resourceManager.firewoodItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "saw pit":
                    Il2CppSystem.Collections.Generic.List<SawPitBuilding> sawPitBuildings = gameManager.resourceManager.sawPitBuildings;
                    foreach (SawPitBuilding sawPitBuilding in sawPitBuildings)
                    {
                        sawPitBuilding.SetWorkEnabled(gameManager.resourceManager.plankItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "tannery":
                    Il2CppSystem.Collections.Generic.List<Tannery> tanneries = gameManager.resourceManager.tanneries;
                    foreach (Tannery tannery in tanneries)
                    {
                        tannery.SetWorkEnabled(gameManager.resourceManager.hideCoatItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "cobbler shop":
                    Il2CppSystem.Collections.Generic.List<CobblerShop> cobblerShops = gameManager.resourceManager.cobblerShops;
                    foreach (CobblerShop cobblerShop in cobblerShops)
                    {
                        cobblerShop.SetWorkEnabled(gameManager.resourceManager.shoesItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "basket shop":
                    Il2CppSystem.Collections.Generic.List<BasketShop> basketShops = gameManager.resourceManager.basketShops;
                    foreach (BasketShop basketShop in basketShops)
                    {
                        basketShop.SetWorkEnabled(gameManager.resourceManager.basketItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "potter building":
                    Il2CppSystem.Collections.Generic.List<PotterBuilding> potterBuildings = gameManager.resourceManager.potterBuildings;
                    foreach (PotterBuilding potterBuilding in potterBuildings)
                    {
                        potterBuilding.SetWorkEnabled(gameManager.resourceManager.potteryItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "candle shop":
                    Il2CppSystem.Collections.Generic.List<CandleShop> candleShops = gameManager.resourceManager.candleShops;
                    foreach (CandleShop candleShop in candleShops)
                    {
                        candleShop.SetWorkEnabled(gameManager.resourceManager.candleItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "soap shop":
                    Il2CppSystem.Collections.Generic.List<SoapShop> soapShops = gameManager.resourceManager.soapShops;
                    foreach (SoapShop soapShop in soapShops)
                    {
                        soapShop.SetWorkEnabled(gameManager.resourceManager.soapItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "furniture workshop":
                    Il2CppSystem.Collections.Generic.List<FurnitureWorkshop> furnitureWorkshops = gameManager.resourceManager.furnitureWorkshops;
                    foreach (FurnitureWorkshop furnitureWorkshop in furnitureWorkshops)
                    {
                        furnitureWorkshop.SetWorkEnabled(gameManager.resourceManager.furnitureItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "brewery":
                    Il2CppSystem.Collections.Generic.List<Brewery> breweries = gameManager.resourceManager.breweries;
                    foreach (Brewery brewery in breweries)
                    {
                        brewery.SetWorkEnabled(gameManager.resourceManager.wheatBeerItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "brickyard":
                    Il2CppSystem.Collections.Generic.List<Brickyard> brickyards = gameManager.resourceManager.brickyards;
                    foreach (Brickyard brickyard in brickyards)
                    {
                        brickyard.SetWorkEnabled(gameManager.resourceManager.brickItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                case "glassmaker":
                    Il2CppSystem.Collections.Generic.List<Glassmaker> glassmakers = gameManager.resourceManager.glassmakers;
                    foreach (Glassmaker glassmaker in glassmakers)
                    {
                        glassmaker.SetWorkEnabled(gameManager.resourceManager.glassItemInfo.unusedCount < resourceCap.Amount, true);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
