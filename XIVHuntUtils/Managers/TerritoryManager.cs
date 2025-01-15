using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DitzyExtensions;
using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using Lumina.Excel.Sheets;
using XIVHuntUtils.Models;
using static DitzyExtensions.EnumExtensions;
using Aetheryte = Lumina.Excel.Sheets.Aetheryte;

namespace XIVHuntUtils.Managers;

public class TerritoryManager : ITerritoryManager {
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
			.ToResult<uint, string>($"Failed to find a territoryId for territory name: {territoryName}");

	public Maybe<string> FindTerritoryName(uint territoryId) =>
		_idToName
			.MaybeGet(_pluginInterface.UiLanguage)
			.Bind(nameMap => nameMap.MaybeGet(territoryId));

	public Result<string, string> GetTerritoryName(uint territoryId) =>
		FindTerritoryName(territoryId)
			.ToResult<string, string>($"Failed to find a territoryName for territory id: {territoryId}");

	public IEnumerable<(Territory territory, uint territoryId)> GetTerritoryIds() =>
		GetEnumValues<Territory>()
			.SelectResults(
				territory => GetTerritoryId(territory.Name())
					.Map(territoryId => (territory, territoryId))
			)
			.ForEachError(error => _log.Debug(error))
			.Value;

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
			.Where(place => supportedMapNames.Contains(place.Name.ToString().AsLower()))
			.ForEach(place => _log.Verbose("Found PlaceName: {0} | {1:l}", place.RowId, place.Name))
			.Select(place => place.RowId)
			.ToImmutableHashSet();

		var dataDicts = GetEnumValues<ClientLanguage>()
			.Select(
				language => {
					var aetheryteTerritories = dataManager.GetExcelSheet<Aetheryte>(language)
						.Where(aetheryte => aetheryte is { IsAetheryte: true, PlaceName.RowId: > 1, Territory.IsValid: true })
						.Where(aetheryte => supportedPlaceIds.Contains(aetheryte.Territory.Value.PlaceName.RowId))
						.Select(aetheryte => aetheryte.Territory.Value)
						.AsList();

					var idToName = dataManager
						.GetExcelSheet<TerritoryType>(language)!
						.Where(territory => territory.TerritoryIntendedUse.RowId == 1)
						.Where(territory => supportedPlaceIds.Contains(territory.PlaceName.RowId))
						.Concat(aetheryteTerritories)
						.Select(territory => (mapId: territory.RowId, name: territory.PlaceName.Value.Name.ToString()))
						.GroupBy(map => map.name)
						.Select(
							grouping => {
								if (1 < grouping.Count()) {
									var dedupedGrouping = grouping.ToImmutableHashSet();
									if (1 < dedupedGrouping.Count) {
										_log.Debug(
											"[{2:l}] Duplicate maps found for name [{0:l}]: {1:l}",
											grouping.Key,
											dedupedGrouping.Select(place => place.mapId.ToString()).Join(", "),
											language.GetLanguageCode()
										);
									}
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
	private static readonly IDictionary<ClientLanguage, string> LangCodes = new Dictionary<ClientLanguage, string>() {
		{ ClientLanguage.Japanese, "jp" },
		{ ClientLanguage.English, "en" },
		{ ClientLanguage.German, "de" },
		{ ClientLanguage.French, "fr" },
	}.VerifyEnumDictionary();

	public static string GetLanguageCode(this ClientLanguage language) => LangCodes[language];
}
