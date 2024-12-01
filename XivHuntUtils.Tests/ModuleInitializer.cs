using System.Runtime.CompilerServices;
using HarmonyLib;

namespace XivHuntUtils.Tests;

public static class ModuleInitializer {
	[ModuleInitializer]
	public static void Init() {
		var harmony = new Harmony("XHUTests");
		harmony.PatchAll();
	}
}
