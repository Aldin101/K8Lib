using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace K8Lib
{
    public class InventoryManager
    {
        private const int customIconCodeStart = 248581;
        private static List<InventoryIcon> customIcons = new List<InventoryIcon>();

        public void checkElements()
        {
            if (K8Lib.GM == null) return;

            GameObject GTTOD = K8Lib.GM.gameObject;
            if (GTTOD == null)
            {
                Debug.LogError("GTTOD not found");
                return;
            }

            GameObject inventoryGrid = K8Lib.GM.gameObject.GetComponent<GTTOD_MemoryManager>().Inventory.GridContent.gameObject;
            bool found = false;
            for (int i = 0; i < inventoryGrid.transform.childCount; i++)
            {
                GTTOD_GridItemUI griditem = inventoryGrid.transform.GetChild(i).GetComponent<GTTOD_GridItemUI>();
                if (griditem == null) continue;
                if (AccessTools.Field(typeof(GTTOD_GridItemUI), "HeldItemIndex").GetValue(griditem) is int itemCode)
                {
                    if (itemCode >= customIconCodeStart && itemCode <= customIconCodeStart + customIcons.Count)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (found) return;

            customIcons.Clear();
        }

        public class InventoryIcon
        {
            public GameObject gameObject;
            public string objectName;
            public int ammount;
            public int itemCode;
            public Action onClick;
            public InventoryIcon(string objectName, string name, string description, int ammount, Sprite icon, Action onClick)
            {
                foreach (InventoryIcon inv in customIcons)
                {
                    if (inv.objectName == objectName)
                    {
                        return;
                    }
                }

                itemCode = customIconCodeStart + customIcons.Count;
                this.objectName = objectName;
                this.ammount = ammount;
                this.onClick = onClick;

                if (K8Lib.GM == null) return;

                GameObject GTTOD = K8Lib.GM.gameObject;
                if (GTTOD == null)
                {
                    Debug.LogError("GTTOD not found");
                    return;
                }

                GameObject inventoryGrid = K8Lib.GM.gameObject.GetComponent<GTTOD_MemoryManager>().Inventory.GridContent.gameObject;

                if (inventoryGrid == null)
                {
                    Debug.LogError("Inventory not found");
                    return;
                }

                GameObject gridItemPrefab = K8Lib.GM.gameObject.GetComponent<GTTOD_MemoryManager>().Inventory.ItemUI.gameObject;

                GameObject gridItem = GameObject.Instantiate(gridItemPrefab, inventoryGrid.transform);
                
                GTTOD_GridItemUI gridItemUI = gridItem.GetComponent<GTTOD_GridItemUI>();

                gridItemUI.SetUpIcon(name, description, icon, ammount, itemCode);

                AccessTools.Field(typeof(GTTOD_GridItemUI), "HeldItemIndex").SetValue(gridItemUI, itemCode);

                if (ammount == -1)
                {
                    Text itemCountText = AccessTools.Field(typeof(GTTOD_GridItemUI), "ItemCountText").GetValue(gridItemUI) as Text;
                    itemCountText.text = "";
                }

                gameObject = gridItem;
                customIcons.Add(this);
            }

            public void setAmmount(int ammount)
            {
                ammount = this.ammount;

                if (ammount == -1)
                {
                    Text itemCountText = AccessTools.Field(typeof(GTTOD_GridItemUI), "ItemCountText").GetValue(gameObject.GetComponent<GTTOD_GridItemUI>()) as Text;
                    itemCountText.text = "";
                } else
                {
                    Text itemCountText = AccessTools.Field(typeof(GTTOD_GridItemUI), "ItemCountText").GetValue(gameObject.GetComponent<GTTOD_GridItemUI>()) as Text;
                    itemCountText.text = "x" + ammount.ToString();
                }
            }

            public void removeIcon()
            {
                customIcons.Remove(this);
                GameObject.Destroy(gameObject);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(GTTOD_GridItemUI), "UseItem")]
        private class GTTOD_GridItemUI_UseItem
        {
            static bool Prefix(GTTOD_GridItemUI __instance)
            {
                int itemCode = (int)AccessTools.Field(typeof(GTTOD_GridItemUI), "HeldItemIndex").GetValue(__instance);
                if (itemCode >= customIconCodeStart && itemCode <= customIconCodeStart + customIcons.Count)
                {
                    customIcons[itemCode - customIconCodeStart].onClick();
                    return false;
                }
                return true;
            }
        }
    }
}