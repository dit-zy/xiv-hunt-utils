using System.Numerics;
using Newtonsoft.Json;
using XIVHuntUtils.Utils;

namespace XIVHuntUtils.Models.Json;

public class HuntDataJsonSpawnPointPosition {
	[JsonProperty("x")] public float X { get; set; } = 0;
	[JsonProperty("y")] public float Y { get; set; } = 0;
	[JsonProperty("z")] public float Z { get; set; } = 0;

	public Vector3 AsVector() => MathUtils.V3(X, Y, Z);
};
