using System.Numerics;
using Newtonsoft.Json;
using XIVHuntUtils.Utils;

namespace XIVHuntUtils.Models.Json;

public class TravelDataJsonAetheryte : JsonPosition {
	[JsonProperty("isTravelNode")] public bool IsTravelNode { get; set; } = true;
};
