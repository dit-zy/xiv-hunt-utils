using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DitzyExtensions;
using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using Lumina.Excel.GeneratedSheets2;
using XIVHuntUtils.Models;
using static DitzyExtensions.EnumExtensions;

namespace XIVHuntUtils.Managers;

public class TerritoryManager {
	private readonly IDalamudPluginInterface _pluginInterface;
	private readonly IPluginLog _log;

	private readonly IDictionary<string, uint> _nameToId;
	private readonly IDictionary<string, IDictionary<uint, string>> _idToName;

	public TerritoryManager(IDalamudPluginInterface pluginInterface, IPluginLog log, IDataManager dataManager) {
		_pluginInterface = pluginInterface;
		_log = log;

		(_nameToId, _idToName) = LoadData(dataManager);
	}

	public Maybe<uint> FindTerritoryId(string territoryName) => _nameToId.MaybeGet(territoryName.AsLower());

	public Result<uint, string> GetTerritoryId(string territoryName) =>
		FindTerritoryId(territoryName)
			.ToResult<uint, string>($"Failed to find a territoryId for map name: {territoryName}");

	public Maybe<string> GetTerritoryName(uint territoryId) =>
		_idToName
			.MaybeGet(_pluginInterface.UiLanguage)
			.Bind(nameMap => nameMap.MaybeGet(territoryId));

	public IEnumerable<(Territory territory, uint territoryId)> GetTerritoryIds() =>
		GetEnumValues<Territory>()
			.SelectResults(
				territory => GetTerritoryId(territory.Name())
					.Map(territoryId => (territory, territoryId))
			)
			.ForEachError(error => _log.Debug(error))
			.Values;

	public IEnumerable<(uint territoryId, uint instances)> GetDefaultInstancesForIds() =>
		GetTerritoryIds()
			.Select(
				territory => (territory.territoryId, territory.territory.DefaultInstances())
			);

	private (IDictionary<string, uint> nameToId, IDictionary<string, IDictionary<uint, string>> idToName) LoadData(
		IDataManager dataManager
	) {
		_log.Debug("Building map data from game files...");

		var supportedMapNames = GetEnumValues<Patch>()
			.SelectMany(patch => patch.HuntMaps())
			.Select(map => map.Name())
			.ToImmutableHashSet();

		var supportedPlaceIds = dataManager.GetExcelSheet<PlaceName>(ClientLanguage.English)!
			.Where(place => supportedMapNames.Contains(place.Name.RawString.AsLower()))
			.ForEach(place => _log.Verbose("Found PlaceName: {0} | {1:l}", place.RowId, place.Name))
			.Select(place => place.RowId)
			.ToImmutableHashSet();

		var dataDicts = GetEnumValues<ClientLanguage>()
			.Select(
				language => {
					var placeNames = dataManager
						.GetExcelSheet<PlaceName>(language)!
						.Where(name => supportedPlaceIds.Contains(name.RowId))
						.Select(name => (name.RowId, name.Name.RawString))
						.AsDict();

					var idToName = dataManager
						.GetExcelSheet<TerritoryType>(language)!
						.Where(territory => territory.TerritoryIntendedUse.Row == 1)
						.Where(territory => placeNames.ContainsKey(territory.PlaceName.Row))
						.Select(territory => (mapId: territory.RowId, name: placeNames[territory.PlaceName.Row]))
						.GroupBy(map => map.name)
						.Select(
							grouping => {
								if (1 < grouping.Count()) {
									_log.Debug(
										"[{2:l}] Duplicate maps found for name [{0:l}]: {1:l}",
										grouping.Key,
										grouping.Select(place => place.mapId.ToString()).Join(", "),
										language.GetLanguageCode()
									);
								}
								return grouping.First();
							}
						)
						.ForEach(
							territory => _log.Verbose(
								"[{2:l}] Found territoryId [{0}] for place: {1:l}",
								territory.mapId,
								territory.name,
								language.GetLanguageCode()
							)
						)
						.AsDict();

					var nameToId = idToName
						.Flip()
						.Select(entry => (entry.Key.AsLower(), entry.Value))
						.AsDict();

					return (nameToId, (language.GetLanguageCode(), idToName));
				}
			)
			.Unzip(
				(ts, us) => (
					ts
						.SelectMany(nameToId => nameToId.AsPairs())
						.AsDict(),
					us
						.AsDict()
				)
			);

		_log.Debug("Map data built.");

		return dataDicts;
	}
}

internal static class TerritoryManagerExtensions {
	private static IDictionary<ClientLanguage, string> _langCodes = new Dictionary<ClientLanguage, string>() {
		{ ClientLanguage.Japanese, "jp" },
		{ ClientLanguage.English, "en" },
		{ ClientLanguage.German, "de" },
		{ ClientLanguage.French, "fr" },
	}.VerifyEnumDictionary();

	public static string GetLanguageCode(this ClientLanguage language) => _langCodes[language];
}
