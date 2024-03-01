using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace K8Lib
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class K8Lib : BaseUnityPlugin
    {
        public static GameManager GM = GameManager.GM;

        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} V{PluginInfo.PLUGIN_VERSION} has loaded");
        }

        private void Update()
        {
            SettingsManager settingsManager = new SettingsManager();
            settingsManager.scrollFix();
            settingsManager.checkElements();

            InventoryManager inventoryManager = new InventoryManager();
            inventoryManager.checkElements();

            if (GM == null)
            {
                GM = GameManager.GM;
            }
        }

        private void OnDestory()
        {
            Harmony harmony = new HarmonyLib.Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }
}
