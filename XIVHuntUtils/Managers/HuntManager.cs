using System.Numerics;
using CSharpFunctionalExtensions;
using Dalamud.Plugin.Services;
using DitzyExtensions;
using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using Newtonsoft.Json;
using XIVHuntUtils.Models;
using XIVHuntUtils.Models.Json;
using EntitySpawns = (uint id, System.Collections.Generic.IList<System.Numerics.Vector3> spawns);
using EntitySpawnDict = System.Collections.Generic.IDictionary<
	uint, System.Collections.Generic.IList<System.Numerics.Vector3>
>;
using Vec3s = System.Collections.Generic.IList<System.Numerics.Vector3>;

namespace XIVHuntUtils.Managers;

public class HuntManager : IHuntManager {
	private readonly IPluginLog _log;
	private readonly TerritoryManager _territoryManager;
	private readonly MobManager _mobManager;
	private readonly EntitySpawnDict _mobSpawns;
	private readonly EntitySpawnDict _territorySpawns;
	private readonly IDictionary<uint, IList<uint>> _territoryMarks;
	private readonly IDictionary<uint, uint> _mobTerritories;

	public HuntManager(IPluginLog log, TerritoryManager territoryManager, MobManager mobManager) :
		this(
			log,
			territoryManager,
			mobManager,
			typeof(HuntManager).Assembly.GetManifestResourceStream(XivHuntUtilsConstants.HuntDataResourceName)!
		) { }
	
	public HuntManager(IPluginLog log, TerritoryManager territoryManager, MobManager mobManager, string huntDataFilename) :
		this(
			log,
			territoryManager,
			mobManager,
			new FileStream(huntDataFilename, FileMode.Open)
		) { }

	public HuntManager(IPluginLog log, TerritoryManager territoryManager, MobManager mobManager, Stream huntDataStream) {
		_log = log;
		_territoryManager = territoryManager;
		_mobManager = mobManager;

		var loadedData = LoadData(huntDataStream);
		_territorySpawns = loadedData.TerritorySpawns;
		_mobSpawns = loadedData.MobSpawns;
		_territoryMarks = loadedData.TerritoryMarks;
		_mobTerritories = loadedData.MobTerritories;
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

	public Maybe<Vec3s> GetTerritorySpawns(uint territoryId) =>
		_territorySpawns.MaybeGet(territoryId);

	public Result<Vec3s, string> GetTerritorySpawns(string territoryName) =>
		_territoryManager
			.GetTerritoryId(territoryName)
			.Bind(
				territoryId =>
					_territorySpawns
						.MaybeGet(territoryId)
						.ToResult<Vec3s, string>($"no spawns found for territoryName: {territoryName}")
			);

	public Maybe<Vector3> FindNearestSpawn(uint territoryId, Vector2 position) =>
		FindNearestSpawn(territoryId, spawn => (position - spawn.XY()).LengthSquared());

	public Maybe<Vector3> FindNearestSpawn(uint territoryId, Vector3 position) =>
		FindNearestSpawn(territoryId, spawn => (position - spawn).LengthSquared());

	private Maybe<Vector3> FindNearestSpawn(
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

	private LoadedData LoadData(Stream huntDataStream) {
		var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, HuntDataJsonMapData>>>(
			new StreamReader(huntDataStream).ReadToEnd()
		);

		if (data is null) {
			throw new JsonSerializationException("failed to parse hunt data ;-;");
		}

		return data
			.SelectMany(patch => ExtractPatchData(patch.Key, patch.Value))
			.SelectMany(ParseTerritoryData)
			.Select(
				territoryData => (
					territoryData.TerritorySpawns,
					territoryData.MarkSpawns,
					territoryData.TerritoryMarks
				)
			)
			.Select(dicts => dicts.Unzip())
			.Select(
				spawnLists => new LoadedData(
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

	private AccumulatedResults<ParsedData, string> ParseTerritoryData(ExtractedData extractedData) {
		var territoryResult = _territoryManager
			.GetTerritoryId(extractedData.TerritoryName);

		var markSpawnsResults = extractedData.Marks
			.SelectResults(
				mark => _mobManager
					.GetMobId(mark.markName)
					.Map(mobId => (mobId, mark.spawns))
			);

		return territoryResult
			.AsAccumulatedResults()
			.PairWith(markSpawnsResults)
			.WithValue(
				entitySpawns => {
					var (territoryId, markSpawns) = entitySpawns;
					markSpawns = markSpawns.AsList();

					var territoryMarks = (territoryId, markSpawns.SelectFirst().AsList());

					return new ParsedData(
						(territoryId, extractedData.Spawns),
						markSpawns,
						territoryMarks
					);
				}
			);
	}

	private IList<ExtractedData> ExtractPatchData(
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

					return new ExtractedData(
						mapData.Key,
						marks,
						spawns.Values.AsList()
					);
				}
			)
			.AsList();

	private record ParsedData(
		EntitySpawns TerritorySpawns,
		IEnumerable<EntitySpawns> MarkSpawns,
		(uint territoryId, IList<uint> marks) TerritoryMarks
	);

	private record ExtractedData(
		string TerritoryName,
		IList<(string markName, Vec3s spawns)> Marks,
		Vec3s Spawns
	);

	private record LoadedData(
		EntitySpawnDict TerritorySpawns,
		EntitySpawnDict MobSpawns,
		IDictionary<uint, IList<uint>> TerritoryMarks,
		IDictionary<uint, uint> MobTerritories
	);
}
