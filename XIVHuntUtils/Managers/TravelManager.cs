using System.Numerics;
using CSharpFunctionalExtensions;
using Dalamud.Plugin.Services;
using DitzyExtensions;
using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using Newtonsoft.Json;
using XIVHuntUtils.Models;
using XIVHuntUtils.Models.Json;

namespace XIVHuntUtils.Managers;

public class TravelManager : ITravelManager {
	private readonly IPluginLog _log;
	private readonly IDictionary<uint, IList<Aetheryte>> _territoryAetherytes;
	private readonly IDictionary<string, Aetheryte> _aetherytesByName;
	private readonly IDictionary<uint, IList<TravelNode>> _territoryTravelNodes;

	public TravelManager(IPluginLog log, ITerritoryManager territoryManager) : this(
		log,
		territoryManager,
		typeof(TravelManager).Assembly.GetManifestResourceStream(XivHuntUtilsConstants.TravelDataResourceName)!
	) { }

	public TravelManager(IPluginLog log, ITerritoryManager territoryManager, string travelDataFilename) : this(
		log,
		territoryManager,
		new FileStream(travelDataFilename, FileMode.Open)
	) { }

	public TravelManager(IPluginLog log, ITerritoryManager territoryManager, Stream travelDataStream) {
		_log = log;

		(_territoryAetherytes, _aetherytesByName, _territoryTravelNodes) = LoadData(territoryManager, travelDataStream);
	}

	public Maybe<Aetheryte> GetAetheryte(string aetheryteName) =>
		_aetherytesByName.MaybeGet(aetheryteName);

	public IList<Aetheryte> GetAetherytesInTerritory(uint territoryId) =>
		_territoryAetherytes.MaybeGet(territoryId).GetValueOrDefault([]);

	public IList<Aetheryte> GetAllAetherytes() =>
		_aetherytesByName.Values.AsList();

	public IList<TravelNode> GetTravelNodesInTerritory(uint territoryId) =>
		_territoryTravelNodes
			.MaybeGet(territoryId)
			.GetValueOrDefault([]);

	public Maybe<Aetheryte> FindNearestAetheryte2d(uint territoryId, Vector2 position) =>
		FindNearestAetheryte(territoryId, routePos => (position - routePos.XY()).LengthSquared());

	public Maybe<Aetheryte> FindNearestAetheryte3d(uint territoryId, Vector3 position) =>
		FindNearestAetheryte(territoryId, routePos => (position - routePos).LengthSquared());

	private Maybe<Aetheryte> FindNearestAetheryte(
		uint territoryId,
		Func<Vector3, float> distanceFunction
	) => GetAetherytesInTerritory(territoryId)
		.MinBy(node => distanceFunction(node.Position))
		.AsMaybe();

	public Maybe<TravelNode> FindNearestTravelNode2d(uint territoryId, Vector2 position) =>
		FindNearestTravelNode(territoryId, routePos => (position - routePos.XY()).LengthSquared());

	public Maybe<TravelNode> FindNearestTravelNode3d(uint territoryId, Vector3 position) =>
		FindNearestTravelNode(territoryId, routePos => (position - routePos).LengthSquared());

	private Maybe<TravelNode> FindNearestTravelNode(
		uint territoryId,
		Func<Vector3, float> distanceFunction
	) => GetTravelNodesInTerritory(territoryId)
		.MinBy(node => distanceFunction(node.Position))
		.AsMaybe();

	private (IDictionary<uint, IList<Aetheryte>>, IDictionary<string, Aetheryte>, IDictionary<uint, IList<TravelNode>>)
		LoadData(
			ITerritoryManager territoryManager,
			Stream travelDataStream
		) {
		var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, TravelDataJsonMapData>>>(
			new StreamReader(travelDataStream).ReadToEnd()
		);

		if (data is null) {
			throw new JsonSerializationException("failed to parse travel data ;-;");
		}

		var aetherytes = data
			.SelectMany(
				patchData =>
					patchData.Value.SelectResults(map => ExtractAetheryteData(territoryManager, map.Key, map.Value))
			)
			.ForEachError(error => _log.Debug(error))
			.Value
			.Flatten()
			.Flatten()
			.AsList();

		var aetherytesByName = aetherytes.AsDict(aetheryte => aetheryte.Name);
		var aetherytesByTerritory = aetherytes
			.GroupBy(aetheryte => aetheryte.Territory)
			.AsDict(group => group.Key.Id(), group => group.AsList());

		var aetheryteTravelNodes = aetherytes
			.Where(aetheryte => aetheryte.IsTravelNode)
			.Select(
				aetheryte => new TravelNode(
					aetheryte,
					true,
					aetheryte.Territory,
					0f,
					aetheryte.Position,
					""
				)
			)
			.AsList();

		var travelNodesByTerritory = data
			.SelectMany(
				patchData =>
					patchData.Value.SelectMany(
						map => ExtractTravelNodeData(territoryManager, aetherytesByName, map.Key, map.Value)
					)
			)
			.ForEachError(error => _log.Debug(error))
			.Value
			.Flatten()
			.Flatten()
			.Concat(aetheryteTravelNodes)
			.GroupBy(travelNode => travelNode.Territory)
			.AsDict(group => group.Key.Id(), group => group.AsList());

		return (aetherytesByTerritory, aetherytesByName, travelNodesByTerritory);
	}

	private Result<IEnumerable<Aetheryte>, string> ExtractAetheryteData(
		ITerritoryManager territoryManager,
		string mapName,
		TravelDataJsonMapData mapData
	) =>
		territoryManager
			.GetTerritoryId(mapName)
			.Map(
				territoryId => mapData
					.Aetherytes
					.Select(
						(aetheryteName, aetheryteData) => new Aetheryte(
							aetheryteName.AsLower(),
							territoryId.AsTerritory(),
							aetheryteData.AsVector(),
							aetheryteData.IsTravelNode
						)
					)
			);

	private AccumulatedResults<IEnumerable<TravelNode>, string> ExtractTravelNodeData(
		ITerritoryManager territoryManager,
		IDictionary<string, Aetheryte> aetherytes,
		string mapName,
		TravelDataJsonMapData mapData
	) {
		return territoryManager
			.GetTerritoryId(mapName)
			.AsAccumulatedResults()
			.SelectMany(
				territoryId => mapData
					.TravelNodes
					.SelectResults(
						travelNode => aetherytes
							.MaybeGet(travelNode.Aetheryte.AsLower())
							.ToResult<Aetheryte, string>($"no aetheryte found with name: {travelNode.Aetheryte.AsLower()}")
							.Map(
								aetheryte =>
									new TravelNode(
										aetheryte,
										false,
										territoryId.AsTerritory(),
										travelNode.DistanceModifier,
										travelNode.Position.AsVector(),
										travelNode.Path
									)
							)
					)
			);
	}
}
