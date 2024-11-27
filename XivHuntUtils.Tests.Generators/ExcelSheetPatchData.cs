namespace XivHuntUtils.Tests.Generators;

internal record struct ExcelSheetPatchData() {
	public string DecoratedClass { get; set; }
	public string DecoratedClassNamespace { get; set; }
	public string SheetTypeName { get; set; }
	public string SheetTypeNamespace { get; set; }
	public string TargetNamespace { get; set; }
	public EqArray<(string name, string type)> Properties { get; set; }
	public EqArray<(string name, string type)> RowRefs { get; set; }
}
