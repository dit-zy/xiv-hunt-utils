// ReSharper disable InconsistentNaming

using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using static XIVHuntUtils.Models.Territory;

namespace XIVHuntUtils.Models;

public enum Patch {
	ARR,
	HW,
	SB,
	SHB,
	EW,
	DT,
}

public static class PatchExtensions {
	private static readonly IDictionary<Patch, IList<Territory>> PatchHuntMaps = new (Patch, IList<Territory>)[] {
			(Patch.ARR, [
				MiddleLaNoscea, LowerLaNoscea, EasternLaNoscea, WesternLaNoscea, UpperLaNoscea, OuterLaNoscea,
				WesternThanalan, CentralThanalan, EasternThanalan, SouthernThanalan, NorthernThanalan,
				CentralShroud, EastShroud, SouthShroud, NorthShroud,
				CoerthasCentralHighlands, MorDhona
			]),
			(Patch.HW, [
				CoerthasWesternHighlands, TheSeaOfClouds, AzysLla,
				TheDravanianForelands, TheDravanianHinterlands, TheChurningMists
			]),
			(Patch.SB, [
				TheFringes, ThePeaks, TheLochs,
				TheRubySea, Yanxia, TheAzimSteppe
			]),
			(Patch.SHB, [
				Lakeland, Kholusia, AmhAraeng,
				IlMheg, TheRaktikaGreatwood, TheTempest
			]),
			(Patch.EW, [
				Labyrinthos, Thavnair, Garlemald,
				MareLamentorum, Elpis, UltimaThule
			]),
			(Patch.DT, [
				Urqopacha, Kozamauka, YakTel,
				Shaaloani, HeritageFound, LivingMemory
			]),
		}
		.Select(patch => (patch.Item1, patch.Item2.AsList()))
		.AsDict()
		.VerifyEnumDictionary();

	private static readonly IDictionary<Patch, IList<Territory>> PatchNonHuntMaps = new (Patch, IList<Territory>)[] {
			(Patch.ARR, [ LimsaLominsaLowerDecks, UldahStepsOfNald, NewGridania ]),
			(Patch.HW, [ Foundation, Idyllshire ]),
			(Patch.SB, [ Kugane, RhalgrsReach, TheDomanEnclave ]),
			(Patch.SHB, [ TheCrystarium, Eulmore ]),
			(Patch.EW, [ OldSharlayan, RadzAtHan ]),
			(Patch.DT, [ Tuliyollal, SolutionNine ]),
		}
		.Select(patch => (patch.Item1, patch.Item2.AsList()))
		.AsDict()
		.VerifyEnumDictionary();

	private static readonly IDictionary<Patch, IList<Territory>> PatchMaps =
		PatchHuntMaps
			.Concat(PatchNonHuntMaps)
			.AsDict()
			.VerifyEnumDictionary();

	private static readonly IDictionary<Territory, Patch> TerritoryPatches =
		PatchMaps
			.SelectMany(patch => patch.Value.Zip(patch.Key.Repeat(patch.Value.Count)))
			.AsDict()
			.VerifyEnumDictionary();

	private static readonly IDictionary<Patch, string> PatchEmotes = new Dictionary<Patch, string> {
		{ Patch.ARR, ":2x:" },
		{ Patch.HW, ":3x:" },
		{ Patch.SB, ":4x:" },
		{ Patch.SHB, ":5x:" },
		{ Patch.EW, ":6x:" },
		{ Patch.DT, ":7x:" },
	}.VerifyEnumDictionary();

	public static IList<Territory> HuntMaps(this Patch patch) => PatchHuntMaps[patch];
	
	public static IList<Territory> NonHuntMaps(this Patch patch) => PatchNonHuntMaps[patch];
	
	public static IList<Territory> Maps(this Patch patch) => PatchMaps[patch];

	public static Patch ContainingPatch(this Territory territory) => TerritoryPatches[territory];

	public static uint MaxMarks(this Patch patch) {
		if (patch == Patch.ARR) return 17;
		return (uint)patch.HuntMaps().Sum(territory => 2 * territory.Instances());
	}

	public static string Emote(this Patch patch) => PatchEmotes[patch];
}
