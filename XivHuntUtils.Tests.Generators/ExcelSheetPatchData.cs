namespace XivHuntUtils.Tests.Generators;

internal record struct ExcelSheetPatchData() {
	public string DecoratedClass { get; set; } = string.Empty;
	public string DecoratedClassNamespace { get; set; } = string.Empty;
	public string SheetTypeName { get; set; } = string.Empty;
	public string SheetTypeNamespace { get; set; } = string.Empty;
	public string TargetNamespace { get; set; } = string.Empty;
	public EqArray<(string name, string type)> Properties { get; set; } = EqArray<(string name, string type)>.Empty;
	public EqArray<(string name, string type)> RowRefs { get; set; } = EqArray<(string name, string type)>.Empty;
}
