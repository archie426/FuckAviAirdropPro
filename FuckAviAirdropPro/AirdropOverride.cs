using System.Reflection;
using FuckAviLibs.Redirector;
using SDG.Unturned;
using UnityEngine;

namespace FuckAviAirdropPro
{
    public class AirdropOverride
    {
        [Override(typeof(LevelManager), "airdrop", BindingFlags.Public | BindingFlags.Static)]
        public static void OV_airdrop(Vector3 point, ushort id, float speed)
        {
            if (!AirdropPlugin.Conf.NoScheduledAirdrops)
                OverrideUtilities.CallOriginal(null, point, id, speed);
        }

        [Override(typeof(Carepackage), "OnCollisionEnter", BindingFlags.NonPublic | BindingFlags.Instance)]
        private void OnCollisionEnter(Collision collision) { } //Do nothing because we make our own shit    
    }
}