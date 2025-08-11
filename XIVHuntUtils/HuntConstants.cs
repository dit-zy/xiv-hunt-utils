using DitzyExtensions.Collection;
using XIVHuntUtils.Models;

namespace XIVHuntUtils;

public static class HuntConstants {
	public static readonly (string Version, IList<(Territory, uint)> Instances) LatestPatchIncreasedInstances =
		("7.3", new List<(Territory, uint)> {
			(Territory.Shaaloani, 2),
			(Territory.HeritageFound, 3),
			(Territory.LivingMemory, 2),
		}.AsList());
}
