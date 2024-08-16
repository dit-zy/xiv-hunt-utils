using DitzyExtensions.Collection;
using DitzyExtensions.Functional;
using static DitzyExtensions.EnumExtensions;
using static XIVHuntUtils.Models.Territory;

namespace XIVHuntUtils.Models;

public enum Territory {
	// HW
	CoerthasWesternHighlands,
	TheSeaOfClouds,
	AzysLla,
	TheDravanianForelands,
	TheDravanianHinterlands,
	TheChurningMists,

	// SB
	TheFringes,
	ThePeaks,
	TheLochs,
	TheRubySea,
	Yanxia,
	TheAzimSteppe,

	// SHB
	Lakeland,
	Kholusia,
	AmhAraeng,
	IlMheg,
	TheRaktikaGreatwood,
	TheTempest,

	// EW
	Labyrinthos,
	Thavnair,
	Garlemald,
	MareLamentorum,
	Elpis,
	UltimaThule,

	// DT
	Urqopacha,
	Kozamauka,
	YakTel,
	Shaaloani,
	HeritageFound,
	LivingMemory,
}

public static class TerritoryExtensions {
	private static readonly IDictionary<Territory, string> TerritoryNames = new Dictionary<Territory, string>() {
		{ CoerthasWesternHighlands, "coerthas western highlands" },
		{ TheSeaOfClouds, "the sea of clouds" },
		{ AzysLla, "azys lla" },
		{ TheDravanianForelands, "the dravanian forelands" },
		{ TheDravanianHinterlands, "the dravanian hinterlands" },
		{ TheChurningMists, "the churning mists" },

		{ TheFringes, "the fringes" },
		{ ThePeaks, "the peaks" },
		{ TheLochs, "the lochs" },
		{ TheRubySea, "the ruby sea" },
		{ Yanxia, "yanxia" },
		{ TheAzimSteppe, "the azim steppe" },

		{ Lakeland, "lakeland" },
		{ Kholusia, "kholusia" },
		{ AmhAraeng, "amh araeng" },
		{ IlMheg, "il mheg" },
		{ TheRaktikaGreatwood, "the rak'tika greatwood" },
		{ TheTempest, "the tempest" },

		{ Labyrinthos, "labyrinthos" },
		{ Thavnair, "thavnair" },
		{ Garlemald, "garlemald" },
		{ MareLamentorum, "mare lamentorum" },
		{ Elpis, "elpis" },
		{ UltimaThule, "ultima thule" },

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

	public static string Name(this Territory territory) => TerritoryNames[territory];

	public static uint DefaultInstances(this Territory territory) => DefaultTerritoryInstances[territory];

	public static uint Instances(this Territory territory) =>
		_instances
			.GetValueOrDefault(
				_territoryToId.MaybeGet(territory).GetValueOrDefault(0U),
				0U
			);
}
