using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace ProductionCap
{
    public class ProductionCapClass : MelonMod
    {
        static int i = 0;
        public override void OnUpdate()
        {
            GameObject go = GameObject.FindGameObjectWithTag("GameManager");
            if (go != null)
            {
                GameManager gameManager = go.GetComponent<GameManager>();

                if (gameManager != null)
                {
                    if (i % 100 != 0)
                    {
                        i++;
                        base.OnUpdate();
                        return;
                    }
                    if (gameManager.resourceManager.potteryItemInfo.unusedCount > 175)
                    {
                        Il2CppSystem.Collections.Generic.List<PotterBuilding> potterBuildings = gameManager.resourceManager.potterBuildings;
                        MelonLogger.Msg(potterBuildings.ToString());
                        foreach (PotterBuilding potterBuilding in potterBuildings)
                        {
                            potterBuilding.isWorkEnabled = false;
                            potterBuilding.enabled = false;
                            MelonLogger.Msg("Potter building disalbed");
                        }
                    }
                    else
                    {
                        Il2CppSystem.Collections.Generic.List<PotterBuilding> potterBuildings = gameManager.resourceManager.potterBuildings;
                        foreach (PotterBuilding potterBuilding in potterBuildings)
                        {
                            potterBuilding.isWorkEnabled = true;
                            potterBuilding.enabled = true;
                            MelonLogger.Msg("Potter building enabled");
                        }
                    }

                }
            }

            base.OnUpdate();
        }
    }
}
