using AllowAIMeERepair.Scripts;
using Stationeers.Addons;
using UnityEngine;

namespace AlwaysActiveGeysers.Scripts
{
    public class FixOreSensorLenses : IPlugin
    {
        public void OnLoad()
        {
            Debug.Log(ModReference.Name + ": Loaded");
        }

        public void OnUnload()
        {
            Debug.Log(ModReference.Name + ": Unloaded");
        }
    }
}
