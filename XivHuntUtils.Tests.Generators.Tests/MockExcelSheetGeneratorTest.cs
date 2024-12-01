using JetBrains.Annotations;

namespace XivHuntUtils.Tests.Generators.Tests;

[TestSubject(typeof(MockExcelSheetGenerator))]
public class MockExcelSheetGeneratorTest {
	[Fact]
	public Task GeneratesMockExcelSheetExtensionsCorrectly() {
		// The source code to test
		var source = """
			  using XivHuntUtils.Tests.Generators;
				using Lumina.Excel.Sheets;
				
				namespace GenTests.Tests;
			  
			  [MockExcelSheet(typeof(PlaceName))]
			  [MockExcelSheet(typeof(NotoriousMonster))]
			  public static class MockSheets { }
			""";

		// Pass the source code to our helper and snapshot test the output
		return TestHelper.VerifySource(source);
	}
}
