using System.Numerics;

namespace XIVHuntUtils.Models;

public record TravelNode(
	Aetheryte StartingAetheryte,
	bool IsAetheryte,
	Territory Territory,
	float DistanceModifier,
	Vector3 Position,
	string Path
);
