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
	private readonly IDictionary<Territory, IList<Aetheryte>> _aetherytes;
	private readonly IDictionary<string, Aetheryte> _aetherytesByName;
	private readonly IDictionary<Territory, IList<TravelNode>> _travelNodes;

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

		LoadData(territoryManager, travelDataStream);
	}

	public Maybe<Aetheryte> GetAetheryte(string aetheryteName) =>
		_aetherytesByName.MaybeGet(aetheryteName);

	public IList<Aetheryte> GetAetherytesInTerritory(Territory territory) =>
		_aetherytes.MaybeGet(territory).GetValueOrDefault([]);

	public IList<Aetheryte> GetAllAetherytes() =>
		_aetherytesByName.Values.AsList();

	private IList<TravelNode> GetTravelNodes(uint territoryId) =>
		_travelNodes
			.MaybeGet(territoryId.AsTerritory())
			.GetValueOrDefault([]);

	public Maybe<TravelNode> FindNearestTravelNode(uint territoryId, Vector2 position) =>
		FindNearestTravelNode(territoryId, routePos => (position - routePos.XY()).LengthSquared());

	public Maybe<TravelNode> FindNearestTravelNode(uint territoryId, Vector3 position) =>
		FindNearestTravelNode(territoryId, routePos => (position - routePos).LengthSquared());

	private Maybe<TravelNode> FindNearestTravelNode(
		uint territoryId,
		Func<Vector3, float> distanceFunction
	) => GetTravelNodes(territoryId)
		.MinBy(node => distanceFunction(node.Position))
		.AsMaybe();

	private void LoadData(ITerritoryManager territoryManager, Stream travelDataStream) {
		var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, TravelDataJsonMapData>>>(
			new StreamReader(travelDataStream).ReadToEnd()
		);

		if (data is null) {
			throw new JsonSerializationException("failed to parse travel data ;-;");
		}

		var aetherytes = data.Select(patchData => ExtractAetheryteData(territoryManager, patchData.Key, patchData.Value));
	}

	private void ExtractAetheryteData(
		ITerritoryManager territoryManager,
		string patchName,
		IDictionary<string, TravelDataJsonMapData> patchMapsData
	) {
		
	}

	private void ExtractPatchData(
		ITerritoryManager territoryManager,
		string patchName,
		IDictionary<string, TravelDataJsonMapData> patchMapsData
	) {
		if (!patchName.AsEnum<Patch>().TryGetValue(out var patch, out var patchError)) {
			throw new Exception(patchError);
		}

		var q = patchMapsData.Select(
			patchMapData => {
				var mapName = patchMapData.Key;
				var mapTravelData = patchMapData.Value;

				var territoryResult = territoryManager.GetTerritoryId(mapName);
				return territoryResult
					.Map(
						territoryId => {
							var aetherytes = mapTravelData
								.Aetherytes
								.Select(aetheryteData => (name: aetheryteData.Key, position: aetheryteData.Value.AsVector()))
								.AsList();

							return (
								mapName: mapName,
								aetherytes: aetherytes,
								travelNodes: mapTravelData.TravelNodes
							);
						}
					);
			}
		);
	}
}
