﻿using XIVHuntUtils.Models;

namespace XIVHuntUtils;

public static class HuntConstants {
	public static readonly (string Version, IList<(Territory, uint)> Instances) LatestPatchIncreasedInstances =
		("7.251", [
			(Territory.YakTel, 2),
		]);
}
