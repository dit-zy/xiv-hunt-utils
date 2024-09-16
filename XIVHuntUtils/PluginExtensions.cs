using Dalamud.Plugin;

namespace XIVHuntUtils;

public static class PluginExtensions {
	public static string PluginName(this IDalamudPluginInterface pluginInterface) =>
		pluginInterface.Manifest.Name;

	public static string PluginVersion(this IDalamudPluginInterface pluginInterface) =>
		pluginInterface.Manifest.AssemblyVersion.ToString();

	public static string PluginNamespace(this IDalamudPluginInterface pluginInterface) =>
		pluginInterface.PluginName().Replace(" ", "");
}
