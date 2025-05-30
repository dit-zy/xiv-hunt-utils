﻿// ReSharper disable InconsistentNaming

using System.Numerics;
using DitzyExtensions;
using XIVHuntUtils.Models;
using static XIVHuntUtils.Utils.MathUtils;

namespace XIVHuntUtils.Utils;

public static class XivUtils {
	#region coord transforms

	public static float TerritoryScale(bool isHwTerritory = false) =>
		isHwTerritory ? 95f : 100f;

	public static bool IsHwTerritory(this uint territoryId) =>
		territoryId.AsTerritory().IsHwTerritory();

	public static bool IsHwTerritory(this Territory territory) =>
		territory.ContainingPatch() == Patch.HW;

	public static float AsMapXY(this float rawCoordinate, uint territoryId) =>
		rawCoordinate.AsMapXY(territoryId.AsTerritory());

	public static float AsMapXY(this float rawCoordinate, Territory territory) =>
		rawCoordinate.AsMapXY(territory.IsHwTerritory());

	public static float AsMapXY(this float rawCoordinate, bool isInHwTerritory = false) =>
		2048 / TerritoryScale(isInHwTerritory) + rawCoordinate / 50f + 1f;

	public static float AsMapZ(this float rawY, float territoryZOffset) =>
		(rawY - territoryZOffset) / 100f;

	public static float AsRawXZ(this float mapCoordinate, uint territoryId) =>
		mapCoordinate.AsRawXZ(territoryId.AsTerritory());

	public static float AsRawXZ(this float mapCoordinate, Territory territory) =>
		mapCoordinate.AsRawXZ(territory.IsHwTerritory());

	public static float AsRawXZ(this float mapCoordinate, bool isInHwTerritory = false) =>
		50f * (mapCoordinate - 1f - 2048 / TerritoryScale(isInHwTerritory));

	public static float AsRawY(this float mapZ, float territoryZOffset) =>
		mapZ * 100 + territoryZOffset;

	public static Vector2 AsMapPosition(this Vector2 rawPosition, uint territoryId) =>
		rawPosition.AsMapPosition(territoryId.AsTerritory());

	public static Vector2 AsMapPosition(this Vector2 rawPosition, Territory territory) =>
		rawPosition.AsMapPosition(territory.IsHwTerritory());

	public static Vector2 AsMapPosition(this Vector2 rawPosition, bool isInHwTerritory = false) =>
		AsMapPosition(rawPosition.X, rawPosition.Y, isInHwTerritory);

	public static Vector2 AsMapPosition(float rawX, float rawZ, uint territoryId) =>
		AsMapPosition(rawX, rawZ, territoryId.AsTerritory());

	public static Vector2 AsMapPosition(float rawX, float rawZ, Territory territory) =>
		AsMapPosition(rawX, rawZ, territory.IsHwTerritory());

	public static Vector2 AsMapPosition(float rawX, float rawZ, bool isInHwTerritory = false) =>
		V2(rawX.AsMapXY(isInHwTerritory), rawZ.AsMapXY(isInHwTerritory));

	public static Vector3 AsMapPosition(this Vector3 rawPosition, float territoryZOffset, uint territoryId) =>
		rawPosition.AsMapPosition(territoryZOffset, territoryId.AsTerritory());

	public static Vector3 AsMapPosition(this Vector3 rawPosition, float territoryZOffset, Territory territory) =>
		rawPosition.AsMapPosition(territoryZOffset, territory.IsHwTerritory());

	public static Vector3 AsMapPosition(this Vector3 rawPosition, float territoryZOffset, bool isInHwTerritory = false) =>
		AsMapPosition(rawPosition.X, rawPosition.Y, rawPosition.Z, territoryZOffset, isInHwTerritory);

	public static Vector3 AsMapPosition(
		float rawX,
		float rawY,
		float rawZ,
		float territoryZOffset,
		uint territoryId
	) =>
		AsMapPosition(rawX, rawY, rawZ, territoryZOffset, territoryId.AsTerritory());

	public static Vector3 AsMapPosition(
		float rawX,
		float rawY,
		float rawZ,
		float territoryZOffset,
		Territory territory
	) =>
		AsMapPosition(rawX, rawY, rawZ, territoryZOffset, territory.IsHwTerritory());

	public static Vector3 AsMapPosition(
		float rawX,
		float rawY,
		float rawZ,
		float territoryZOffset,
		bool isInHwTerritory = false
	) =>
		V3(rawX.AsMapXY(isInHwTerritory), rawZ.AsMapXY(isInHwTerritory), rawY.AsMapZ(territoryZOffset));

	public static Vector2 AsRawPosition(this Vector2 mapPosition, uint territoryId) =>
		mapPosition.AsRawPosition(territoryId.AsTerritory());

	public static Vector2 AsRawPosition(this Vector2 mapPosition, Territory territory) =>
		mapPosition.AsRawPosition(territory.IsHwTerritory());

	public static Vector2 AsRawPosition(this Vector2 mapPosition, bool isInHwTerritory = false) =>
		AsRawPosition(mapPosition.X, mapPosition.Y, isInHwTerritory);

	public static Vector2 AsRawPosition(float mapX, float mapY, uint territoryId) =>
		AsRawPosition(mapX, mapY, territoryId.AsTerritory());

	public static Vector2 AsRawPosition(float mapX, float mapY, Territory territory) =>
		AsRawPosition(mapX, mapY, territory.IsHwTerritory());

	public static Vector2 AsRawPosition(float mapX, float mapY, bool isInHwTerritory = false) =>
		V2(mapX.AsRawXZ(isInHwTerritory), mapY.AsRawXZ(isInHwTerritory));

	public static Vector3 AsRawPosition(this Vector3 mapPosition, float territoryZOffset, uint territoryId) =>
		mapPosition.AsRawPosition(territoryZOffset, territoryId.AsTerritory());

	public static Vector3 AsRawPosition(this Vector3 mapPosition, float territoryZOffset, Territory territory) =>
		mapPosition.AsRawPosition(territoryZOffset, territory.IsHwTerritory());

	public static Vector3 AsRawPosition(this Vector3 mapPosition, float territoryZOffset, bool isInHwTerritory = false) =>
		AsRawPosition(mapPosition.X, mapPosition.Y, mapPosition.Z, territoryZOffset, isInHwTerritory);

	public static Vector3 AsRawPosition(
		float mapX,
		float mapY,
		float mapZ,
		float territoryZOffset,
		uint territoryId
	) =>
		AsRawPosition(mapX, mapY, mapZ, territoryZOffset, territoryId.AsTerritory());

	public static Vector3 AsRawPosition(
		float mapX,
		float mapY,
		float mapZ,
		float territoryZOffset,
		Territory territory
	) =>
		AsRawPosition(mapX, mapY, mapZ, territoryZOffset, territory.IsHwTerritory());

	public static Vector3 AsRawPosition(
		float mapX,
		float mapY,
		float mapZ,
		float territoryZOffset,
		bool isInHwTerritory = false
	) =>
		V3(mapX.AsRawXZ(isInHwTerritory), mapZ.AsRawY(territoryZOffset), mapY.AsRawXZ(isInHwTerritory));

	#endregion

	public static Patch AsPatch(this string patchName) => patchName.AsEnum<Patch>();
}
