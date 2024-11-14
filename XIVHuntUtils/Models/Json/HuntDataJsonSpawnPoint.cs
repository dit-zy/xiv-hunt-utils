using System.Numerics;
using Newtonsoft.Json;

namespace XIVHuntUtils.Models.Json;

public class HuntDataJsonSpawnPoint {
	[JsonProperty("id")] public uint Id { get; set; } = 0;
	[JsonProperty("position")] public JsonPosition Position { get; set; } = null!;

	public (uint, Vector3) AsPair() => (Id, Position.AsVector());
};
