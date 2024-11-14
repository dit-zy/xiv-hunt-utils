using Newtonsoft.Json;

namespace XIVHuntUtils.Models.Json;

public class TravelDataJsonMapData {
	[JsonProperty("aetherytes")]
	public IDictionary<string, JsonPosition> Aetherytes { get; set; } = new Dictionary<string, JsonPosition>();

	[JsonProperty("travelNodes")] public IList<TravelDataJsonTravelNode> TravelNodes { get; set; } = [];
};
