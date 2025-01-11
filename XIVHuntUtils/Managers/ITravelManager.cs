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

	public IList<TravelNode> GetTravelNodesInTerritory(uint territoryId);

	public Maybe<Aetheryte> FindNearestAetheryte2d(uint territoryId, float x, float y) =>
		FindNearestAetheryte2d(territoryId, new Vector2(x, y));

	public Maybe<Aetheryte> FindNearestAetheryte2d(uint territoryId, Vector2 position);

	public Maybe<Aetheryte> FindNearestAetheryte3d(uint territoryId, float x, float y, float z) =>
		FindNearestAetheryte3d(territoryId, new Vector3(x, y, z));

	public Maybe<Aetheryte> FindNearestAetheryte3d(uint territoryId, Vector3 position);

	public Maybe<TravelNode> FindNearestTravelNode2d(uint territoryId, float x, float y) =>
		FindNearestTravelNode2d(territoryId, new Vector2(x, y));

	public Maybe<TravelNode> FindNearestTravelNode2d(uint territoryId, Vector2 position);

	public Maybe<TravelNode> FindNearestTravelNode3d(uint territoryId, float x, float y, float z) =>
		FindNearestTravelNode3d(territoryId, new Vector3(x, y, z));

	public Maybe<TravelNode> FindNearestTravelNode3d(uint territoryId, Vector3 position);
}
