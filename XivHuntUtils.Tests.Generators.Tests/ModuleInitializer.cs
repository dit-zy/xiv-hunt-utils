﻿using System.Runtime.CompilerServices;

namespace XivHuntUtils.Tests.Generators.Tests;

public static class ModuleInitializer {
	[ModuleInitializer]
	public static void Init() {
		VerifySourceGenerators.Initialize();
	}
}
