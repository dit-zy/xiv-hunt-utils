using CSharpFunctionalExtensions;
using DitzyExtensions;
using XIVHuntUtils.Models;

namespace XIVHuntUtils.Managers;

public interface IMobManager {
	Maybe<HuntMarkData> FindMob(uint mobId);

	Maybe<HuntMarkData> FindMob(string mobName);

	Maybe<uint> FindMobId(string mobName);

	Maybe<string> FindMobName(uint mobId);

	Maybe<Rank> FindMobRank(uint mobId);

	Maybe<Rank> FindMobRank(string mobName);

	Result<HuntMarkData, string> GetMob(uint mobId);

	Result<HuntMarkData, string> GetMob(string mobName);

	Result<uint, string> GetMobId(string mobName);

	Result<string, string> GetMobName(uint mobId);

	Result<Rank, string> GetMobRank(uint mobId);

	Result<Rank, string> GetMobRank(string mobName);
}
