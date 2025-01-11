using Newtonsoft.Json;

namespace XIVHuntUtils.Models.Json;

public class TravelDataJsonTravelNode {
	[JsonProperty("aetheryte")] public string Aetheryte { get; set; } = "";

	[JsonProperty("distanceModifier")] public float DistanceModifier { get; set; } = 0;

	[JsonProperty("position")] public JsonPosition Position { get; set; } = null!;

	[JsonProperty("path")] public string Path { get; set; } = "";
};
