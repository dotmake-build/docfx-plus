using System;
using System.Reflection;
using System.Xml.Linq;
using HarmonyLib;
// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Patches
{
    //[HarmonyPatch]
    internal static class XmlCommentPatch
    {
        public static Type Type = Assemblies.DocfxDotnetAssembly.GetType("Docfx.Dotnet.XmlComment", true);

        [HarmonyPatch]
        internal static class ResolveCode
        {
            public static MethodBase TargetMethod() => AccessTools.Method(Type, nameof(ResolveCode));

            internal static bool Prefix(XDocument doc, object context, object __instance)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ResolveCode)} Prefix is run!");
                XmlComment.ResolveCode(doc, context, __instance);
                return false; //don't run original method
            }
        }

        /*
        [HarmonyPatch]
        internal static class ResolveCodeSource
        {
            public static MethodBase TargetMethod() => AccessTools.Method(XmlCommentType, nameof(ResolveCodeSource));

            internal static bool Prefix(XElement node, object context, ref (string lang, string code) __result)
            {
                //Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ResolveCodeSource)} Prefix is run!");
                __result = XmlComment.ResolveCodeSource(node, context);
                return false;
            }

            public static void Postfix()
            {
                Console.WriteLine($"{ExecutableInfo.AssemblyInfo.Product}: {nameof(ResolveCodeSource)} Postfix is run!");

                // Your logic after the original method
            }

            public static void Cleanup(Exception exception)
            {
                if (exception != null)
                    Console.WriteLine(exception);
            }
        }
        */
    }
}
