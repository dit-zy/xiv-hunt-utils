using System.Numerics;
using static XIVHuntUtils.Utils.MathUtils;

namespace XIVHuntUtils.Utils;

public static class XivUtils {
	public static float TerritoryScale(bool isHwTerritory = false) =>
		isHwTerritory ? 95f : 100f;

	public static float AsMapXY(this float rawCoordinate, bool isInHwTerritory = false) =>
		2048 / TerritoryScale(isInHwTerritory) + rawCoordinate / 50f + 1f;

	public static float AsMapZ(this float rawY, float territoryZOffset) =>
		(rawY - territoryZOffset) / 100f;

	public static float AsRawXZ(this float mapCoordinate, bool isInHwTerritory = false) =>
		50f * (mapCoordinate - 1f - 2048 / TerritoryScale(isInHwTerritory));

	public static float AsRawY(this float mapZ, float territoryZOffset) =>
		mapZ * 100 + territoryZOffset;

	public static Vector2 AsMapPosition(this Vector2 rawPosition, bool isInHwTerritory = false) =>
		AsMapPosition(rawPosition.X, rawPosition.Y, isInHwTerritory);

	public static Vector2 AsMapPosition(float rawX, float rawZ, bool isInHwTerritory = false) =>
		V2(rawX.AsMapXY(isInHwTerritory), rawZ.AsMapXY(isInHwTerritory));

	public static Vector3 AsMapPosition(this Vector3 rawPosition, float territoryZOffset, bool isInHwTerritory = false) =>
		AsMapPosition(rawPosition.X, rawPosition.Y, rawPosition.Z, territoryZOffset, isInHwTerritory);

	public static Vector3 AsMapPosition(
		float rawX,
		float rawY,
		float rawZ,
		float territoryZOffset,
		bool isInHwTerritory = false
	) =>
		V3(rawX.AsMapXY(isInHwTerritory), rawZ.AsMapXY(isInHwTerritory), rawY.AsMapZ(territoryZOffset));

	public static Vector2 AsRawPosition(this Vector2 mapPosition, bool isInHwTerritory = false) =>
		AsRawPosition(mapPosition.X, mapPosition.Y, isInHwTerritory);

	public static Vector2 AsRawPosition(float mapX, float mapY, bool isInHwTerritory = false) =>
		V2(mapX.AsRawXZ(isInHwTerritory), mapY.AsRawXZ(isInHwTerritory));

	public static Vector3 AsRawPosition(this Vector3 mapPosition, float territoryZOffset, bool isInHwTerritory = false) =>
		AsRawPosition(mapPosition.X, mapPosition.Y, mapPosition.Z, territoryZOffset, isInHwTerritory);

	public static Vector3 AsRawPosition(
		float mapX,
		float mapY,
		float mapZ,
		float territoryZOffset,
		bool isInHwTerritory = false
	) =>
		V3(mapX.AsRawXZ(isInHwTerritory), mapZ.AsRawY(territoryZOffset), mapY.AsRawXZ(isInHwTerritory));
}
