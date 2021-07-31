using System;
using System.IO;
using System.Linq;
using System.Reflection;

using HarmonyLib;
using Replay;
using Replay.Clasz;

namespace Replay.Lib
{

    public static class AdofaiPatch
    {

        public static bool IsValidPatch(Type patchType)
        {
            AdofaiPatchAttribute patchAttr = patchType.GetCustomAttribute<AdofaiPatchAttribute>();
            if (patchAttr == null) return false;
            Type classType = patchAttr.Assembly.GetType(patchAttr.ClassName);
            if(classType==null) return false;
            return Misc.isVaild(patchAttr.MinVersion, patchAttr.MaxVersion);
        }


        public static void Patch(this Harmony harmony, Type patchType)
        {
            AdofaiPatchAttribute metadata = patchType.GetCustomAttribute<AdofaiPatchAttribute>();
            if (metadata == null) return;
            if (metadata.IsEnabled) return;
            
            if (!IsValidPatch(patchType)) return;
            
            if (metadata.alphaType == Alpha.DontUse)
            {
                if (Main.isAlpha) return;
            }
            if (metadata.alphaType == Alpha.Use)
            {
                if (!Main.isAlpha) return;
            }

            Type declaringType = metadata.Assembly.GetType(metadata.ClassName);
            if (declaringType == null) return;

            harmony.CreateClassProcessor(patchType).Patch();
            metadata.IsEnabled = true;
        }


        public static void Unpatch(this Harmony harmony, Type patchType)
        {

            AdofaiPatchAttribute metadata = patchType.GetCustomAttribute<AdofaiPatchAttribute>();
            if (metadata == null) return;
            if (!metadata.IsEnabled) return;

            Type classType = metadata.Assembly.GetType(metadata.ClassName);
            if (classType == null)  return;

            MethodInfo original = metadata.info.method;
            foreach (var patch in patchType.GetMethods()) harmony.Unpatch(original, patch);
           
            metadata.IsEnabled = false;
        }

    }
}