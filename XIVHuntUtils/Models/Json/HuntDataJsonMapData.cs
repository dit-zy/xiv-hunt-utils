using Newtonsoft.Json;

namespace XIVHuntUtils.Models.Json;

public class HuntDataJsonMapData {
	[JsonProperty("marks")]
	public IDictionary<string, HuntDataJsonMark> Marks { get; set; } = new Dictionary<string, HuntDataJsonMark>();

	[JsonProperty("spawns")] public IList<HuntDataJsonSpawnPoint> Spawns { get; set; } = [];
};
