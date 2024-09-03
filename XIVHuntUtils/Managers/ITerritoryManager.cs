using CSharpFunctionalExtensions;
using XIVHuntUtils.Models;

namespace XIVHuntUtils.Managers;

public interface ITerritoryManager {
	public Maybe<uint> FindTerritoryId(string territoryName);

	public Result<uint, string> GetTerritoryId(string territoryName);

	public Maybe<string> FindTerritoryName(uint territoryId);

	public Result<string, string> GetTerritoryName(uint territoryId);

	public IEnumerable<(Territory territory, uint territoryId)> GetTerritoryIds();

	public IEnumerable<(uint territoryId, uint instances)> GetDefaultInstancesForIds();
}
