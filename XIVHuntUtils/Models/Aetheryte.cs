using System.Numerics;

namespace XIVHuntUtils.Models;

public record Aetheryte(
	string Name,
	Territory Territory,
	Vector3 Position,
	bool IsTravelNode
);
