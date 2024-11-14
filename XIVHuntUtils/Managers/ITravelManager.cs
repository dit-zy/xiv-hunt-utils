using System.Numerics;
using CSharpFunctionalExtensions;
using XIVHuntUtils.Models;

namespace XIVHuntUtils.Managers;

public interface ITravelManager {
	public Maybe<Aetheryte> GetAetheryte(string aetheryteName);

	public IList<Aetheryte> GetAetherytesInTerritory(uint territoryId) =>
		GetAetherytesInTerritory(territoryId.AsTerritory());

	public IList<Aetheryte> GetAetherytesInTerritory(Territory territory) =>
		GetAetherytesInTerritory(territory.Id());

	public IList<Aetheryte> GetAllAetherytes();

	public Maybe<TravelNode> FindNearestTravelNode(uint territoryId, float x, float y) =>
		FindNearestTravelNode(territoryId, new Vector2(x, y));

	public Maybe<TravelNode> FindNearestTravelNode(uint territoryId, Vector2 position);

	public Maybe<TravelNode> FindNearestTravelNode(uint territoryId, float x, float y, float z) =>
		FindNearestTravelNode(territoryId, new Vector3(x, y, z));

	public Maybe<TravelNode> FindNearestTravelNode(uint territoryId, Vector3 position);
}
