using System.Numerics;
using CSharpFunctionalExtensions;
using XIVHuntUtils.Models;

namespace XIVHuntUtils.Managers;

public interface IHuntManager {
	public Result<Territory, string> GetMarkTerritory(uint mobId);
	public Result<Territory, string> GetMarkTerritory(string mobName);

	public Result<IList<Vector3>, string> GetMarkSpawns(uint mobId);
	public Result<IList<Vector3>, string> GetMarkSpawns(string mobName);

	public Maybe<IList<Vector3>> GetTerritorySpawns(uint territoryId);
	public Result<IList<Vector3>, string> GetTerritorySpawns(string territoryName);

	public Maybe<Vector3> FindNearestSpawn2d(uint territoryId, float x, float y) =>
		FindNearestSpawn2d(territoryId, new Vector2(x, y));

	public Maybe<Vector3> FindNearestSpawn2d(uint territoryId, Vector2 position);

	public Maybe<Vector3> FindNearestSpawn3d(uint territoryId, float x, float y, float z) =>
		FindNearestSpawn3d(territoryId, new Vector3(x, y, z));

	public Maybe<Vector3> FindNearestSpawn3d(uint territoryId, Vector3 position);
}
