using System.Net.Http.Headers;
using Dalamud.Plugin;

namespace XIVHuntUtils;

public static class WebConstants {
	public static readonly MediaTypeWithQualityHeaderValue MediaTypeJson =
		MediaTypeWithQualityHeaderValue.Parse("application/json");
}

public static class WebConstantExtensions {
	public static ProductInfoHeaderValue PluginUserAgent(this IDalamudPluginInterface pluginInterface) =>
		new(
			pluginInterface.PluginNamespace(),
			pluginInterface.PluginVersion()
		);
}
