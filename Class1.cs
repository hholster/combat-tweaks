using BepInEx;
using HarmonyLib;
using Invector.vCharacterController;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx.Logging;
using Invector.vItemManager;
using System.Collections;
using Invector;
using EZCameraShake;

namespace CombatMod
{
    internal class PluginInfo
    {
        internal const string GUID = "hol.vaproxy.combattweaks";
        internal const string Name = "Combat Tweaks";
        internal const string Version = "1.0.0";
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

            harmony.PatchAll();
            harmony.Patch(healOriginal,new HarmonyMethod(healPatch));
            harmony.Patch(crashOriginal, new HarmonyMethod(crashPatch));
            harmony.Patch(crashOriginal, null, new HarmonyMethod(crashPatchPost));

            harmony.Patch(chargoOriginal, new HarmonyMethod(chargoPatch));
            harmony.Patch(onReceiveDamageOriginal, new HarmonyMethod(onReceiveDamagePatch));
            harmony.Patch(onReceiveDamageOriginal, null, new HarmonyMethod(onReceiveDamagePatchPost));
        }
    }

    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch("FixedUpdate")]
    public class ParryTimePatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand.Equals((sbyte)15))
                {

                    codes[i].opcode = OpCodes.Ldc_I4;
                    codes[i].operand = 7;
                }
                yield return codes[i];
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
            Debug.Log("ORDP Pre!");
            if (damage.damageType == "drone")
            {
                canCharge = 0;
                Debug.Log("(prefix ORDP) canCharge is..."+canCharge);
            }
        }
        public static void OnReceiveDamagePatchPost(vDamage damage)
        {
            Debug.Log("ORDP Post!");
            if (damage.damageType == "drone")
            {
                canCharge = 1;
                Debug.Log("(postfix ORDP) canCharge is..." + canCharge);
            }
        }

        public static bool ChargoPatch()
        {
            Debug.Log("(chargo) canCharge is..." + canCharge);
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