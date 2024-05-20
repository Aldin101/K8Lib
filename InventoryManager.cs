using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace K8Lib.Inventory
{
    internal static class Manager
    {
        public const int customIconCodeStart = 248581;
        public static List<InventoryIcon> customIcons = new List<InventoryIcon>();

        public static void checkElements()
        {
            GameObject GTTOD = GameManager.GM.gameObject;
            if (GTTOD == null)
            {
                Debug.LogError("GTTOD not found");
                return;
            }

            GameObject inventoryGrid = GameManager.GM.GetComponent<GTTOD_MemoryManager>().Inventory.GridContent.gameObject;
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

            restoreElements();
        }

        public static void restoreElements()
        {
            GameObject GTTOD = GameManager.GM.gameObject;
            if (GTTOD == null)
            {
                Debug.LogError("GTTOD not found");
                return;
            }

            GameObject inventoryGrid = GameManager.GM.GetComponent<GTTOD_MemoryManager>().Inventory.GridContent.gameObject;

            if (inventoryGrid == null)
            {
                Debug.LogError("Inventory not found");
                return;
            }

            GameObject gridItemPrefab = GameManager.GM.GetComponent<GTTOD_MemoryManager>().Inventory.ItemUI.gameObject;

            foreach (InventoryIcon icon in customIcons)
            {
                GameObject gridItem = GameObject.Instantiate(gridItemPrefab, inventoryGrid.transform);

                GTTOD_GridItemUI gridItemUI = gridItem.GetComponent<GTTOD_GridItemUI>();

                gridItemUI.SetUpIcon(icon.objectName, "", null, icon.amount == null ? 0 : (int)icon.amount, icon.itemCode);

                AccessTools.Field(typeof(GTTOD_GridItemUI), "HeldItemIndex").SetValue(gridItemUI, icon.itemCode);

                if (icon.amount == null)
                {
                    Text itemCountText = AccessTools.Field(typeof(GTTOD_GridItemUI), "ItemCountText").GetValue(gridItemUI) as Text;
                    itemCountText.text = "";
                }
            }
        }

        [HarmonyPatch(typeof(GTTOD_GridItemUI), "UseItem")]
        private class GTTOD_GridItemUI_UseItem
        {
            static bool Prefix(GTTOD_GridItemUI __instance)
            {
                int itemCode = (int)AccessTools.Field(typeof(GTTOD_GridItemUI), "HeldItemIndex").GetValue(__instance);
                if (itemCode >= Manager.customIconCodeStart && itemCode <= Manager.customIconCodeStart + customIcons.Count)
                {
                    customIcons[itemCode - customIconCodeStart].onClick();
                    return false;
                }
                return true;
            }
        }
    }

    public class InventoryIcon
    {
        public GameObject gameObject;
        public string objectName;
        public string name;
        public string description;
        public Sprite icon;
        public int? amount;
        public int itemCode;
        public Action onClick;
        public InventoryIcon(string objectName, string name, string description, int? amount, Sprite icon, Action onClick)
        {
            foreach (InventoryIcon inv in Manager.customIcons)
            {
                if (inv.objectName == objectName)
                {
                    return;
                }
            }

            itemCode = Manager.customIconCodeStart + Manager.customIcons.Count;
            this.objectName = objectName;
            this.name = name;
            this.description = description;
            this.icon = icon;
            this.amount = amount;
            this.onClick = onClick;

            BepInEx.Bootstrap.Chainloader.ManagerObject.GetComponent<K8Lib>().StartCoroutine(addIcon());
        }

        private IEnumerator addIcon()
        {
            while (GameManager.GM == null) yield return null;

            GameObject inventoryGrid = GameManager.GM.GetComponent<GTTOD_MemoryManager>().Inventory.GridContent.gameObject;

            if (inventoryGrid == null)
            {
                Debug.LogError("Inventory not found");
                yield break;
            }

            GameObject gridItemPrefab = GameManager.GM.GetComponent<GTTOD_MemoryManager>().Inventory.ItemUI.gameObject;

            GameObject gridItem = GameObject.Instantiate(gridItemPrefab, inventoryGrid.transform);

            GTTOD_GridItemUI gridItemUI = gridItem.GetComponent<GTTOD_GridItemUI>();
            gridItemUI.SetUpIcon(name, description, icon, amount != null ? (int)amount : 0, itemCode);

            AccessTools.Field(typeof(GTTOD_GridItemUI), "HeldItemIndex").SetValue(gridItemUI, itemCode);

            if (amount == null)
            {
                Text itemCountText = AccessTools.Field(typeof(GTTOD_GridItemUI), "ItemCountText").GetValue(gridItemUI) as Text;
                itemCountText.text = "";
            }

            gameObject = gridItem;

            Manager.customIcons.Add(this);
        }

        public void setAmount(int? amount)
        {
            amount = this.amount;

            if (amount == null)
            {
                Text itemCountText = AccessTools.Field(typeof(GTTOD_GridItemUI), "ItemCountText").GetValue(gameObject.GetComponent<GTTOD_GridItemUI>()) as Text;
                itemCountText.text = "";
            }
            else
            {
                Text itemCountText = AccessTools.Field(typeof(GTTOD_GridItemUI), "ItemCountText").GetValue(gameObject.GetComponent<GTTOD_GridItemUI>()) as Text;
                itemCountText.text = "x" + amount.ToString();
            }
        }

        public void removeIcon()
        {
            Manager.customIcons.Remove(this);
            GameObject.Destroy(gameObject);
        }
    }
}