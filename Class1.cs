using BepInEx;
using HarmonyLib;
using Invector;
using Rewired;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CombatMod
{
    internal class PluginInfo
    {
        internal const string GUID = "hol.vaproxy.combattweaks";
        internal const string Name = "Combat Tweaks";
        internal const string Version = "1.0.1";
    }

    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            Harmony harmony = new Harmony(PluginInfo.GUID);

            MethodInfo crashOriginal = AccessTools.Method(typeof(Gimmicks), "Crash");

            MethodInfo crashPatch = AccessTools.Method(typeof(ParryGroundPatch), "CrashPatch");

            MethodInfo crashPatchPost = AccessTools.Method(typeof(ParryGroundPatch), "CrashPatchPost");

            MethodInfo healOriginal = AccessTools.Method(typeof(Inventory), "ParryHeal");

            MethodInfo healPatch = AccessTools.Method(typeof(ParryGroundPatch), "ParryHealPatch");

            MethodInfo onReceiveDamageOriginal = AccessTools.Method(typeof(vHitDamageParticle), "OnReceiveDamage");

            MethodInfo onReceiveDamagePatch = AccessTools.Method(typeof(DroneChargePatch), "OnReceiveDamagePatch");

            MethodInfo onReceiveDamagePatchPost = AccessTools.Method(typeof(DroneChargePatch), "OnReceiveDamagePatchPost");

            MethodInfo chargoOriginal = AccessTools.Method(typeof(Drone), "Chargo");

            MethodInfo chargoPatch = AccessTools.Method(typeof(DroneChargePatch), "ChargoPatch");

            MethodInfo updateOriginal = AccessTools.Method(typeof(Inventory), "Update");

            MethodInfo updatePrefix = AccessTools.Method(typeof(ParryTimePatch), "UpdatePrefix");

            MethodInfo fixedUpdateOriginal = AccessTools.Method(typeof(Inventory), "FixedUpdate");

            MethodInfo fixedUpdatePrefix = AccessTools.Method(typeof(ParryTimePatch), "FixedUpdatePrefix");


            harmony.Patch(updateOriginal, new HarmonyMethod(updatePrefix));
            harmony.Patch(fixedUpdateOriginal, new HarmonyMethod(fixedUpdatePrefix));

            harmony.Patch(healOriginal, new HarmonyMethod(healPatch));
            harmony.Patch(crashOriginal, new HarmonyMethod(crashPatch));
            harmony.Patch(crashOriginal, null, new HarmonyMethod(crashPatchPost));

            harmony.Patch(chargoOriginal, new HarmonyMethod(chargoPatch));
            harmony.Patch(onReceiveDamageOriginal, new HarmonyMethod(onReceiveDamagePatch));
            harmony.Patch(onReceiveDamageOriginal, null, new HarmonyMethod(onReceiveDamagePatchPost));
        }
    }

    public class ParryTimePatch
    {
        public static int parryWindow;
        internal static float timeWindow = 0.75f;
        internal static List<float> parryPressTimes = new List<float>();
        public static void UpdatePrefix(Inventory __instance)
        {
            parryWindow = 7;

            float currentTime = Time.time;

            parryPressTimes.RemoveAll(time => currentTime - time > timeWindow);

            if (ReInput.players.GetPlayer("Player0").GetButtonDown("Parry") && !__instance.enemyride)
            {
                parryPressTimes.Add(currentTime);
                __instance.AttackHold = 0;
            }
            int buttonPressCount = parryPressTimes.Count(time => currentTime - time <= timeWindow);


            if (__instance.AttackHold < buttonPressCount)
            {
                __instance.AttackHold = buttonPressCount;
            }
            if (buttonPressCount > 1)
            {
                parryWindow -= buttonPressCount;
            }
        }

        public static void FixedUpdatePrefix(Inventory __instance)
        {
            if (__instance.AttackHold > parryWindow && __instance.AttackHold < 15)
            {
                __instance.AttackHold = 100;
            }
        }
    }

    class ParryGroundPatch
    {
        internal static int canHeal = 1;
        public static void CrashPatch()
        {
            canHeal = 0;
        }
        public static void CrashPatchPost()
        {
            canHeal = 1;
        }
        public static bool ParryHealPatch()
        {
            if (canHeal == 1)
            {
                return true;
            }
            else
            {
                return false;

            }
        }
    }

    class DroneChargePatch
    {
        internal static int canCharge = 1;
        public static void OnReceiveDamagePatch(vDamage damage)
        {
            if (damage.damageType == "drone")
            {
                canCharge = 0;
            }
        }
        public static void OnReceiveDamagePatchPost(vDamage damage)
        {
            if (damage.damageType == "drone")
            {
                canCharge = 1;
            }
        }

        public static bool ChargoPatch()
        {
            if (canCharge == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}