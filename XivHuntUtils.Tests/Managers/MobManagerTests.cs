using CSharpFunctionalExtensions;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using FluentAssertions;
using JetBrains.Annotations;
using Lumina.Excel.Sheets;
using Moq;
using XIVHuntUtils.Managers;
using XivHuntUtils.Tests.MoqUtils;
using static XivHuntUtils.Tests.MoqUtils.MoqHelper;

namespace XivHuntUtils.Tests.Managers;

[TestSubject(typeof(MobManager))]
public class MobManagerTests {
	private readonly Mock<IPluginLog> _log = new();
	private readonly Mock<IDataManager> _dataManager = new();

	public MobManagerTests() { }

	[Fact]
	void ManagerIsInitializedAsExpected() {
		// DATA
		var mockNmSheet = CreateMockSheet<NotoriousMonster>();
		mockNmSheet.AddMockRows(
			MockNotoriousMonster.Create(8).SetBNpcNameRowId(82),
			MockNotoriousMonster.Create(37).SetBNpcNameRowId(214),
			MockNotoriousMonster.Create(58).SetBNpcNameRowId(233)
		);
		var mockBNpcNameSheet = CreateMockSheet<BNpcName>();
		mockBNpcNameSheet.AddMockRows(
			MockBNpcName.Create(82).SetSingular(LuminaSeString("Hisoka")),
			MockBNpcName.Create(165).SetSingular(LuminaSeString("Mime")),
			MockBNpcName.Create(214).SetSingular(LuminaSeString("Regular Clown")),
			MockBNpcName.Create(233).SetSingular(LuminaSeString("Hisoka"))
		);

		// GIVEN
		_dataManager
			.Setup(dm => dm.GetExcelSheet<NotoriousMonster>(It.IsAny<ClientLanguage>(), null))
			.Returns(mockNmSheet);
		_dataManager
			.Setup(dm => dm.GetExcelSheet<BNpcName>(It.IsAny<ClientLanguage>(), null))
			.Returns(mockBNpcNameSheet);

		// WHEN
		var manager = new MobManager(_log.Object, _dataManager.Object);

		// THEN
		_log.Verify(
			log => log.Debug(
				"Duplicate mobs found for name [{0:l}]: {1:l}",
				"hisoka",
				"82, 233"
			)
		);

		manager.FindMobId("Hisoka").Should().Be(Maybe.From(82u));
		manager.FindMobId("Regular Clown").Should().Be(Maybe.From(214u));
		manager.FindMobId("Mine").Should().Be(Maybe<uint>.None);
		manager.FindMobName(82).Should().Be(Maybe.From("hisoka"));
		manager.FindMobName(165).Should().Be(Maybe<string>.None);
		manager.FindMobName(214).Should().Be(Maybe.From("regular clown"));
		manager.FindMobName(233).Should().Be(Maybe<string>.None);
	}
}
