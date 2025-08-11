using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using DitzyExtensions;
using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using Lumina.Excel.Sheets;
using XIVHuntUtils.Models;

namespace XIVHuntUtils.Managers;

public class MobManager : IMobManager {
	private readonly IPluginLog _log;

	private readonly IDictionary<string, HuntMarkData> _mobByName;
	private readonly IDictionary<uint, HuntMarkData> _mobById;
	private readonly IMultiDict<Rank, HuntMarkData> _mobsByRank;

	public MobManager(IPluginLog log, IDataManager dataManager) {
		_log = log;

		var loadedData = LoadData(dataManager);
		_mobById = loadedData.ById;
		_mobByName = loadedData.ByName;
		_mobsByRank = loadedData.ByRank;
	}

	public Maybe<HuntMarkData> FindMob(uint mobId) => _mobById.MaybeGet(mobId);

	public Maybe<HuntMarkData> FindMob(string mobName) => _mobByName.MaybeGet(mobName.AsLower());

	public Maybe<uint> FindMobId(string mobName) => FindMob(mobName).Select(mob => mob.Id);

	public Maybe<string> FindMobName(uint mobId) => FindMob(mobId).Select(mob => mob.Name);
	
	public Maybe<Rank> FindMobRank(uint mobId) => FindMob(mobId).Select(mob => mob.Rank);
	
	public Maybe<Rank> FindMobRank(string mobName) => FindMob(mobName).Select(mob => mob.Rank);

	public Result<HuntMarkData, string> GetMob(uint mobId) =>
		FindMob(mobId)
			.ToResult<HuntMarkData, string>($"Failed to find a mob with id: {mobId}");

	public Result<HuntMarkData, string> GetMob(string mobName) =>
		FindMob(mobName)
			.ToResult<HuntMarkData, string>($"Failed to find a mob with name: {mobName}");

	public Result<uint, string> GetMobId(string mobName) => GetMob(mobName).Map(mob => mob.Id);
	
	public Result<string, string> GetMobName(uint mobId) => GetMob(mobId).Map(mob => mob.Name);

	public Result<Rank, string> GetMobRank(string mobName) => GetMob(mobName).Map(mob => mob.Rank);
	
	public Result<Rank, string> GetMobRank(uint mobId) => GetMob(mobId).Map(mob => mob.Rank);

	private LoadedData LoadData(IDataManager dataManager) {
		_log.Debug("Building mob data from game files...");

		var notoriousMonsters = dataManager.GetExcelSheet<NotoriousMonster>(ClientLanguage.English)!
			.Select(monster => (monster.BNpcName.RowId, monster.Rank))
			.Where(monster => monster.Rank is > 0 and <= 3 && monster.RowId != 0)
			.AsDict();

		var ranks = EnumExtensions.GetEnumValues<Rank>();
		var mobs = dataManager.GetExcelSheet<BNpcName>(ClientLanguage.English)!
			.Select(name => (name: name.Singular.ToString().AsLower(), mobId: name.RowId))
			.Where(name => notoriousMonsters.ContainsKey(name.mobId))
			.Select(entry => new HuntMarkData(
					entry.mobId,
					entry.name,
					ranks[notoriousMonsters[entry.mobId] - 1]
				)
			)
			.GroupBy(mob => mob.Name)
			.Select(grouping => {
					if (1 < grouping.Count()) {
						_log.Debug(
							"Duplicate mobs found for name [{0:l}]: {1:l}",
							grouping.Key,
							grouping.Select(mob => mob.Id.ToString()).Join(", ")
						);
					}
					return grouping.First();
				}
			)
			.AsList();

		_log.Debug("Mob data built.");

		return new LoadedData(
			mobs.AsDict(mob => mob.Id),
			mobs.AsDict(mob => mob.Name),
			mobs.AsMultiDict(mob => mob.Rank)
		);
	}

	private record LoadedData(
		IDictionary<uint, HuntMarkData> ById,
		IDictionary<string, HuntMarkData> ByName,
		IMultiDict<Rank, HuntMarkData> ByRank
	);
}
