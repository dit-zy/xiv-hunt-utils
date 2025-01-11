using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using static DitzyExtensions.EnumExtensions;
using static XIVHuntUtils.Models.Territory;

namespace XIVHuntUtils.Models;

public enum Territory {
	// ARR
	LimsaLominsaLowerDecks,
	UldahStepsOfNald,
	NewGridania,
	MiddleLaNoscea,
	LowerLaNoscea,
	EasternLaNoscea,
	WesternLaNoscea,
	UpperLaNoscea,
	OuterLaNoscea,
	WesternThanalan,
	CentralThanalan,
	EasternThanalan,
	SouthernThanalan,
	NorthernThanalan,
	CentralShroud,
	EastShroud,
	SouthShroud,
	NorthShroud,
	CoerthasCentralHighlands,
	MorDhona,

	// HW
	Foundation,
	Idyllshire,
	CoerthasWesternHighlands,
	TheSeaOfClouds,
	AzysLla,
	TheDravanianForelands,
	TheDravanianHinterlands,
	TheChurningMists,

	// SB
	Kugane,
	RhalgrsReach,
	TheDomanEnclave,
	TheFringes,
	ThePeaks,
	TheLochs,
	TheRubySea,
	Yanxia,
	TheAzimSteppe,

	// SHB
	TheCrystarium,
	Eulmore,
	Lakeland,
	Kholusia,
	AmhAraeng,
	IlMheg,
	TheRaktikaGreatwood,
	TheTempest,

	// EW
	OldSharlayan,
	RadzAtHan,
	Labyrinthos,
	Thavnair,
	Garlemald,
	MareLamentorum,
	Elpis,
	UltimaThule,

	// DT
	Tuliyollal,
	SolutionNine,
	Urqopacha,
	Kozamauka,
	YakTel,
	Shaaloani,
	HeritageFound,
	LivingMemory,
}

public static class TerritoryExtensions {
	private static readonly IDictionary<Territory, string> TerritoryNames = new Dictionary<Territory, string>() {
		{ LimsaLominsaLowerDecks, "limsa lominsa lower decks" },
		{ UldahStepsOfNald, "ul'dah - steps of nald" },
		{ NewGridania, "new gridania" },
		{ MiddleLaNoscea, "middle la noscea" },
		{ LowerLaNoscea, "lower la noscea" },
		{ EasternLaNoscea, "eastern la noscea" },
		{ WesternLaNoscea, "western la noscea" },
		{ UpperLaNoscea, "upper la noscea" },
		{ OuterLaNoscea, "outer la noscea" },
		{ WesternThanalan, "western thanalan" },
		{ CentralThanalan, "central thanalan" },
		{ EasternThanalan, "eastern thanalan" },
		{ SouthernThanalan, "southern thanalan" },
		{ NorthernThanalan, "northern thanalan" },
		{ CentralShroud, "central shroud" },
		{ EastShroud, "east shroud" },
		{ SouthShroud, "south shroud" },
		{ NorthShroud, "north shroud" },
		{ CoerthasCentralHighlands, "coerthas central highlands" },
		{ MorDhona, "mor dhona" },
		
		{ Foundation, "foundation" },
		{ Idyllshire, "idyllshire" },
		{ CoerthasWesternHighlands, "coerthas western highlands" },
		{ TheSeaOfClouds, "the sea of clouds" },
		{ AzysLla, "azys lla" },
		{ TheDravanianForelands, "the dravanian forelands" },
		{ TheDravanianHinterlands, "the dravanian hinterlands" },
		{ TheChurningMists, "the churning mists" },

		{ Kugane, "kugane" },
		{ RhalgrsReach, "rhalgr's reach" },
		{ TheDomanEnclave, "the doman enclave" },
		{ TheFringes, "the fringes" },
		{ ThePeaks, "the peaks" },
		{ TheLochs, "the lochs" },
		{ TheRubySea, "the ruby sea" },
		{ Yanxia, "yanxia" },
		{ TheAzimSteppe, "the azim steppe" },

		{ TheCrystarium, "the crystarium" },
		{ Eulmore, "eulmore" },
		{ Lakeland, "lakeland" },
		{ Kholusia, "kholusia" },
		{ AmhAraeng, "amh araeng" },
		{ IlMheg, "il mheg" },
		{ TheRaktikaGreatwood, "the rak'tika greatwood" },
		{ TheTempest, "the tempest" },

		{ OldSharlayan, "old sharlayan" },
		{ RadzAtHan, "radz-at-han" },
		{ Labyrinthos, "labyrinthos" },
		{ Thavnair, "thavnair" },
		{ Garlemald, "garlemald" },
		{ MareLamentorum, "mare lamentorum" },
		{ Elpis, "elpis" },
		{ UltimaThule, "ultima thule" },

		{ Tuliyollal, "tuliyollal" },
		{ SolutionNine, "solution nine" },
		{ Urqopacha, "urqopacha" },
		{ Kozamauka, "kozama'uka" },
		{ YakTel, "yak t'el" },
		{ Shaaloani, "shaaloani" },
		{ HeritageFound, "heritage found" },
		{ LivingMemory, "living memory" },
	}.VerifyEnumDictionary();

	private static readonly IDictionary<Territory, uint> DefaultTerritoryInstances =
		GetEnumValues<Territory>()
			.Select(territory => (territory, 1U))
			.Concat(HuntConstants.LatestPatchIncreasedInstances.Instances)
			.AsDict();

	private static IDictionary<uint, uint> _instances = null!;
	private static IDictionary<Territory, uint> _territoryToId = null!;
	private static IDictionary<uint, Territory> _idToTerritory = null!;

	public static void SetTerritoryInstances(
		IDictionary<uint, uint> instances,
		IEnumerable<(Territory territory, uint id)> territoryIds
	) {
		if (_instances is not null)
			throw new Exception("cannot set territory instance dictionary after plugin initialization.");

		_instances = instances;
		_territoryToId = territoryIds.AsDict();
		_idToTerritory = _territoryToId.Flip();
	}

	public static Territory AsTerritory(this uint territoryId) => _idToTerritory[territoryId];

	public static uint Id(this Territory territory) => _territoryToId[territory];

	public static string Name(this Territory territory) => TerritoryNames[territory];

	public static uint DefaultInstances(this Territory territory) => DefaultTerritoryInstances[territory];

	public static uint Instances(this Territory territory) =>
		_instances
			.GetValueOrDefault(
				_territoryToId.MaybeGet(territory).GetValueOrDefault(0U),
				0U
			);
}
