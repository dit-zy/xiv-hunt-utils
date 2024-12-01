﻿//HintName: MockExcelSheetAttribute.g.cs
// <autogenerated/>
using System;

namespace XivHuntUtils.Tests.Generators;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class MockExcelSheetAttribute : System.Attribute {

	public Type SheetType { get; set; }
	public string TargetNamespace { get; set; }
	
	public MockExcelSheetAttribute(Type sheetType) {
		SheetType = sheetType;
		TargetNamespace = default;
	}
}