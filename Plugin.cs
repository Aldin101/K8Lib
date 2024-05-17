using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace K8Lib
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class K8Lib : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} has loaded");
        }

        private void Update()
        {
            if (!GameManager.GM) return;
            Settings.Manager.scrollFix();
            Settings.Manager.checkElements();

            Inventory.Manager.checkElements();
        }

        private void OnDestroy()
        {
            Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }
}
