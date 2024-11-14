using System.Numerics;

namespace XIVHuntUtils.Models;

public record TravelNode(
	Aetheryte Aetheryte,
	float DistanceModifier,
	Vector3 Position,
	string Path
);
