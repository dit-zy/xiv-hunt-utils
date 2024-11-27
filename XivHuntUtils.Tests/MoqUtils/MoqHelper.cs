using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace XivHuntUtils.Tests.MoqUtils;

public static class MoqHelper {
	public static ExcelSheet<T> CreateMockSheet<T>() where T : struct, IExcelRow<T> => new(null!);

	public static ReadOnlySeString LuminaSeString(string str) => new(str);
}
