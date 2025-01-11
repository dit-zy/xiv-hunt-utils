using Newtonsoft.Json;

namespace XIVHuntUtils.Models.Json;

public class TravelDataJsonMapData {
	[JsonProperty("aetherytes")]
	public IDictionary<string, TravelDataJsonAetheryte> Aetherytes { get; set; } = new Dictionary<string, TravelDataJsonAetheryte>();

	[JsonProperty("travelNodes")] public IList<TravelDataJsonTravelNode> TravelNodes { get; set; } = [];
};
