using UnityEngine;
using HarmonyLib;
using BepInEx;

namespace K8Lib
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} V{PluginInfo.PLUGIN_VERSION} has loaded");
        }

        private void Update()
        {
            SettingsManager settingsManager = new SettingsManager();
            settingsManager.scrollFix();

            if (SettingsManager.GM == null)
            {
                SettingsManager.GM = GameManager.GM;
            }
        }

        private void OnDestory()
        {
            Harmony harmony = new HarmonyLib.Harmony(PluginInfo.PLUGIN_GUID);
            harmony.UnpatchSelf();
        }
    }
}
