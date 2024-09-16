using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using DitzyExtensions;
using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using Lumina.Excel.GeneratedSheets2;

namespace XIVHuntUtils.Managers;

public class MobManager : IMobManager {
	private readonly IPluginLog _log;

	private readonly IDictionary<string, uint> _mobNameToId;
	private readonly IDictionary<uint, string> _mobIdToName;

	public MobManager(IPluginLog log, IDataManager dataManager) {
		_log = log;

		(_mobNameToId, _mobIdToName) = LoadData(dataManager);
	}

	public Maybe<uint> FindMobId(string mobName) => _mobNameToId.MaybeGet(mobName.AsLower());

	public Result<uint, string> GetMobId(string mobName) =>
		FindMobId(mobName)
			.ToResult<uint, string>($"Failed to find a mobId for mob name: {mobName}");

	public Maybe<string> FindMobName(uint mobId) => _mobIdToName.MaybeGet(mobId);
	public Result<string, string> GetMobName(uint mobId) => 
		FindMobName(mobId)
			.ToResult<string, string>($"Failed to find a mobName for mob id: {mobId}");

	private (IDictionary<string, uint> nameToId, IDictionary<uint, string> idToName) LoadData(
		IDataManager dataManager
	) {
		_log.Debug("Building mob data from game files...");

		var notoriousMonsters = dataManager.GetExcelSheet<NotoriousMonster>(ClientLanguage.English)!
			.Select(monster => monster.BNpcName.Row)
			.ToImmutableHashSet();

		var nameToId = dataManager.GetExcelSheet<BNpcName>(ClientLanguage.English)!
			.Select(name => (name: name.Singular.RawString.AsLower(), mobId: name.RowId))
			.Where(name => notoriousMonsters.Contains(name.mobId))
			.GroupBy(entry => entry.name)
			.Select(
				grouping => {
					if (1 < grouping.Count()) {
						_log.Debug(
							"Duplicate mobs found for name [{0:l}]: {1:l}",
							grouping.Key,
							grouping.Select(entry => entry.mobId.ToString()).Join(", ")
						);
					}
					return grouping.First();
				}
			)
			.AsDict();
		var idToName = nameToId.Flip();

		_log.Debug("Mob data built.");

		return (nameToId, idToName);
	}
}
