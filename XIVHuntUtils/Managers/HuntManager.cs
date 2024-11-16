using System.Numerics;
using System.Reflection;
using System.Resources;
using CSharpFunctionalExtensions;
using Dalamud.Plugin.Services;
using DitzyExtensions;
using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using Newtonsoft.Json;
using XIVHuntUtils.Models;
using XIVHuntUtils.Models.Json;
using Vec3s = System.Collections.Generic.IList<System.Numerics.Vector3>;
using EntitySpawns = (uint id, System.Collections.Generic.IList<System.Numerics.Vector3> spawns);
using EntitySpawnDict
	= System.Collections.Generic.IDictionary<uint, System.Collections.Generic.IList<System.Numerics.Vector3>>;

namespace XIVHuntUtils.Managers;

public class HuntManager : IHuntManager {
	private readonly IPluginLog _log;
	private readonly TerritoryManager _territoryManager;
	private readonly MobManager _mobManager;
	private readonly EntitySpawnDict _mobSpawns;
	private readonly EntitySpawnDict _territorySpawns;
	private readonly IDictionary<uint, IList<uint>> _territoryMarks;
	private readonly IDictionary<uint, uint> _mobTerritories;

	public HuntManager(IPluginLog log, TerritoryManager territoryManager, MobManager mobManager) {
		_log = log;
		_territoryManager = territoryManager;
		_mobManager = mobManager;

		(_territorySpawns, _mobSpawns, _territoryMarks, _mobTerritories) = LoadData();
	}

	public Result<Territory, string> GetMarkTerritory(uint mobId) {
		return _mobTerritories
			.MaybeGet(mobId)
			.ToResult<uint, string>($"no territory found for mobId: {mobId}")
			.Map(territoryId => territoryId.AsTerritory());
	}

	public Result<Territory, string> GetMarkTerritory(string mobName) {
		return _mobManager
			.GetMobId(mobName)
			.Bind(
				mobId => _mobTerritories
					.MaybeGet(mobId)
					.ToResult<uint, string>($"no territory found for mobId: {mobId}")
					.Map(territoryId => territoryId.AsTerritory())
			);
	}

	public Result<Vec3s, string> GetMarkSpawns(uint mobId) =>
		_mobSpawns
			.MaybeGet(mobId)
			.ToResult<Vec3s, string>($"no spawns found for mobId: {mobId}");

	public Result<Vec3s, string> GetMarkSpawns(string mobName) =>
		_mobManager
			.GetMobId(mobName)
			.Bind(
				mobId =>
					_mobSpawns
						.MaybeGet(mobId)
						.ToResult<Vec3s, string>($"no spawns found for mobName: {mobName}")
			);

	public Result<Vec3s, string> GetTerritorySpawns(uint territoryId) =>
		_territorySpawns
			.MaybeGet(territoryId)
			.ToResult<Vec3s, string>($"no spawns found for territoryId: {territoryId}");

	public Result<Vec3s, string> GetTerritorySpawns(string territoryName) =>
		_territoryManager
			.GetTerritoryId(territoryName)
			.Bind(
				territoryId =>
					_territorySpawns
						.MaybeGet(territoryId)
						.ToResult<Vec3s, string>($"no spawns found for territoryName: {territoryName}")
			);

	public Result<Vector3, string> FindNearestSpawn(uint territoryId, Vector2 position) =>
		FindNearestSpawn(territoryId, spawn => (position - spawn.XY()).LengthSquared());

	public Result<Vector3, string> FindNearestSpawn(uint territoryId, Vector3 position) =>
		FindNearestSpawn(territoryId, spawn => (position - spawn).LengthSquared());

	private Result<Vector3, string> FindNearestSpawn(
		uint territoryId,
		Func<Vector3, float> distanceFunction
	) =>
		GetTerritorySpawns(territoryId)
			.Map(
				spawns => spawns
					.Select(spawn => (spawn, distance: distanceFunction(spawn)))
					.MinBy(spawn => spawn.distance)
					.spawn
			);

	private (EntitySpawnDict, EntitySpawnDict, IDictionary<uint, IList<uint>>, IDictionary<uint, uint>) LoadData() {
		var huntDataStream = GetType().Assembly.GetManifestResourceStream(XivHuntUtilsConstants.HuntDataResourceName);
		if (huntDataStream is null) {
			throw new MissingManifestResourceException(
				$"could not initialize {nameof(HuntManager)}. resource [{XivHuntUtilsConstants.HuntDataResourceName}] not found >_>?"
			);
		}
		
		var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, HuntDataJsonMapData>>>(
			new StreamReader(huntDataStream).ReadToEnd()
		);
		
		if (data is null) {
			throw new JsonSerializationException("failed to parse hunt data ;-;");
		}

		return data
			.SelectMany(patch => ExtractPatchData(patch.Key, patch.Value))
			.SelectMany(
				territoryData => ParseTerritoryData(territoryData.territoryName, territoryData.marks, territoryData.spawns)
			)
			.Select(dicts => dicts.Unzip())
			.Select(
				spawnLists => (
					spawnLists.ts.AsDict(),
					spawnLists.us.Flatten().AsDict(),
					spawnLists.vs.AsDict(),
					spawnLists.vs.SelectMany(
						territoryMarks => territoryMarks.marks.Select(markId => (markId, territoryMarks.territoryId))
					).AsDict()
				)
			)
			.ForEachError(error => _log.Error(error))
			.Value;
	}

	private AccumulatedResults<(EntitySpawns, IEnumerable<EntitySpawns>, (uint territoryId, IList<uint> marks)), string>
		ParseTerritoryData(
			string territoryName,
			IList<(string markName, Vec3s spawns)> marks,
			Vec3s spawns
		) {
		var territorySpawnsResults = _territoryManager
			.GetTerritoryId(territoryName)
			.Map(territoryId => (territoryId, spawns));

		var markSpawnsResults = marks
			.SelectResults(
				mark => _mobManager
					.GetMobId(mark.markName)
					.Map(mobId => (mobId, mark.spawns))
			);

		return territorySpawnsResults
			.AsAccumulatedResults()
			.PairWith(markSpawnsResults)
			.WithValue(
				entitySpawns => {
					var (territorySpawns, markSpawns) = entitySpawns;
					markSpawns = markSpawns.AsList();
					var territoryMarks = (territorySpawns.territoryId, markSpawns.SelectFirst().AsList());
					return (territorySpawns, markSpawns, territoryMarks);
				}
			);
	}

	private IList<(string territoryName, IList<(string markName, Vec3s spawns)> marks, Vec3s spawns)> ExtractPatchData(
		string patchName,
		IDictionary<string, HuntDataJsonMapData> patchMapData
	) =>
		patchMapData.Select(
				mapData => {
					var spawns = mapData
						.Value
						.Spawns
						.Select(spawnPoint => spawnPoint.AsPair())
						.AsDict();

					var marks = mapData
						.Value
						.Marks
						.Select(
							mark => (
								markName: mark.Key,
								spawns: mark
									.Value
									.Spawns
									.Select(spawnId => spawns[spawnId])
									.AsList()
							)
						)
						.AsList();

					return (territoryName: mapData.Key, marks: marks, spawns: spawns.Values.AsList());
				}
			)
			.AsList();
}
