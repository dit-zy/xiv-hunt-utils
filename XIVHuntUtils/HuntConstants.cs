using XIVHuntUtils.Models;

namespace XIVHuntUtils;

public static class HuntConstants {
	public static readonly (string Version, IList<(Territory, uint)> Instances) LatestPatchIncreasedInstances =
		("7.2", [
			(Territory.Urqopacha, 1),
			(Territory.Kozamauka, 1),
			(Territory.YakTel, 1),
			(Territory.Shaaloani, 1),
			(Territory.HeritageFound, 2),
			(Territory.LivingMemory, 1)
		]);
}
