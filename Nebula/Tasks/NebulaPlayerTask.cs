﻿using UnhollowerRuntimeLib;

namespace Nebula.Tasks;

[HarmonyPatch]
public class NormalPlayerTaskPatch
{
    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.AppendTaskText))]
    class AppendTaskTextPatch
    {
        static public bool Prefix(NormalPlayerTask __instance, [HarmonyArgument(0)] Il2CppSystem.Text.StringBuilder sb)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return true;
            t.__AppendTaskText(sb);
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.IsComplete), MethodType.Getter)]
    class ISCompletePatch
    {
        static public bool Prefix(NormalPlayerTask __instance, ref bool __result)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return true;
            __result = t.__IsCompleted();
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.NextStep))]
    class NextStepPatch
    {
        static public bool Prefix(NormalPlayerTask __instance)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return true;
            t.__NextStep();
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
    class InitializePatch
    {
        static public bool Prefix(NormalPlayerTask __instance)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return true;
            t.__Initialize();
            t.VanilaStructData = new UnhollowerBaseLib.Il2CppStructArray<byte>(8);
            __instance.Data = t.VanilaStructData;
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.FixedUpdate))]
    class UpdatePatch
    {
        static public void Postfix(NormalPlayerTask __instance)
        {
            if (__instance.TaskType != TaskTypes.None) return;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return;
            t.__Update();
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.UpdateArrow))]
    class UpdateArrowPatch
    {
        static public bool Prefix(NormalPlayerTask __instance)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return true;
            t.__UpdateArrow();
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.ValidConsole))]
    class ValidConsolePatch
    {
        static public bool Prefix(NormalPlayerTask __instance, ref bool __result, [HarmonyArgument(0)] Console console)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return true;
            __result = t.__ValidConsole(console);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerTask), nameof(PlayerTask.Locations), MethodType.Getter)]
    class GetLocationsPatch
    {
        static public bool Prefix(PlayerTask __instance, ref Il2CppSystem.Collections.Generic.List<Vector2> __result)
        {
            if (__instance.TaskType != TaskTypes.None) return true;
            NebulaPlayerTask t = __instance.GetComponent<NebulaPlayerTask>();
            if (!t) return true;
            t.LocationDirty = false;
            __result = null;
            t.__GetLocations(ref __result);
            return false;
        }
    }
}


public class NebulaPlayerTask : NormalPlayerTask
{

    static NebulaPlayerTask()
    {
        ClassInjector.RegisterTypeInIl2Cpp<NebulaPlayerTask>();
    }

    public NebulaPlayerTask()
    {
        this.TaskType = TaskTypes.None;
    }

    public virtual void __AppendTaskText(Il2CppSystem.Text.StringBuilder sb) { }

    public virtual void __NextStep() { }


    public virtual void __Initialize() { }

    public virtual void __Update() { }

    public virtual void __UpdateArrow() { }

    public virtual bool __ValidConsole(Console console) { return false; }

    public virtual bool __IsCompleted()
    {
        return false;
    }

    public virtual void __GetLocations(ref Il2CppSystem.Collections.Generic.List<Vector2> __result) { }


    public byte[] NebulaData;
    public UnhollowerBaseLib.Il2CppStructArray<byte> VanilaStructData;
}
