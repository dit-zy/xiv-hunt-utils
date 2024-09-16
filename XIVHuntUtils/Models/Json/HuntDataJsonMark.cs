using Newtonsoft.Json;

namespace XIVHuntUtils.Models.Json;

public class HuntDataJsonMark {
	[JsonProperty("spawns")] public IList<uint> Spawns { get; set; } = [];
};
