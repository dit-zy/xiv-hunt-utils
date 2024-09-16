using CSharpFunctionalExtensions;
using DitzyExtensions;

namespace XIVHuntUtils.Managers;

public interface IMobManager {
	public Maybe<uint> FindMobId(string mobName);

	public Result<uint, string> GetMobId(string mobName);

	public Maybe<string> FindMobName(uint mobId);

	public Result<string, string> GetMobName(uint mobId);
}
