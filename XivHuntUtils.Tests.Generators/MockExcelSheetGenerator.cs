using System.Text;
using DitzyExtensions;
using DitzyExtensions.Collection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static XivHuntUtils.Tests.Generators.GenUtils;

namespace XivHuntUtils.Tests.Generators;

[Generator]
public class MockExcelSheetGenerator : IIncrementalGenerator {
	public void Initialize(IncrementalGeneratorInitializationContext context) {
		context.RegisterPostInitializationOutput(
			ctx => {
				ctx.AddSource("MockExcelSheetAttribute.g.cs", SourceText.From(MockExcelSheetAttributeSource, Encoding.UTF8));
				ctx.AddSource("StaticInstanceField.g.cs", SourceText.From(StaticInstanceFieldSource, Encoding.UTF8));
			}
		);

		var sheetPatchData = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				MockExcelSheetAttributeFqn,
				predicate: static (_, _) => true,
				transform: static (ctx, _) => GetEnumToGenerate(ctx.SemanticModel, ctx.TargetNode)
			)
			.Where(static m => m is not null);

		context.RegisterSourceOutput(
			sheetPatchData,
			static (ctx, data) => GenerateSheetPatches(ctx, data)
		);
	}

	private static void GenerateSheetPatches(SourceProductionContext ctx, List<ExcelSheetPatchData> data) {
		if (data.IsEmpty()) return;

		data.ForEach(
			sheetData => {
				var className = $"Mock{sheetData.SheetTypeName}Extensions";
				ctx.AddSource(
					$"{className}.g.cs",
					SourceText.From(GetExtensionClassText(sheetData, className), Encoding.UTF8)
				);
			}
		);
	}

	private static List<ExcelSheetPatchData> GetEnumToGenerate(SemanticModel semanticModel, SyntaxNode targetNode) {
		if (semanticModel.GetDeclaredSymbol(targetNode) is not INamedTypeSymbol classSymbol) {
			return [];
		}

		var attributeType = semanticModel.Compilation.GetTypeByMetadataName(MockExcelSheetAttributeFqn);
		if (attributeType is null) return [];

		var sheetPatchData = new List<ExcelSheetPatchData>();

		INamedTypeSymbol? sheetType = null;

		foreach (var attributeData in classSymbol.GetAttributes()) {
			if (!attributeType.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default)) {
				continue;
			}

			var e = new ExcelSheetPatchData {
				DecoratedClass = classSymbol.Name,
				DecoratedClassNamespace = classSymbol.ContainingNamespace.ToDisplayString()
			};

			var constructorArgs = attributeData.ConstructorArguments;
			for (var i = 0; i < constructorArgs.Length; i++) {
				var arg = constructorArgs[i];
				if (arg.Kind == TypedConstantKind.Error) return [];

				switch (i) {
					case 0:
						sheetType = (arg.Value as INamedTypeSymbol)!;
						break;
					case 1:
						e.TargetNamespace = (arg.Value as string)!;
						break;
				}
			}

			foreach (var arg in attributeData.NamedArguments) {
				if (arg.Value.Kind == TypedConstantKind.Error) return [];

				switch (arg.Key) {
					case "SheetType":
						if (arg.Value.Value is INamedTypeSymbol st) {
							sheetType = st;
						}
						break;
					case "TargetNamespace":
						if (arg.Value.Value is string targetNamespace) {
							e.TargetNamespace = targetNamespace;
						}
						break;
				}
			}

			if (sheetType is null) continue;

			e.SheetTypeName = sheetType.Name;
			e.SheetTypeNamespace = sheetType.ContainingNamespace.ToDisplayString();

			var members = sheetType.GetMembers();
			var props = new List<(string, string)>(members.Length);
			var rowRefs = new List<(string, string)>(members.Length);
			foreach (var sym in members) {
				if (sym is not IPropertySymbol propSym) continue;
				if ("rowid".Equals(propSym.Name.AsLower())) continue;

				var prop = (propSym.Name, propSym.Type.ToDisplayString());
				props.Add(prop);
				if (propSym.Type.Name.AsLower() == "rowref") rowRefs.Add(prop);
			}
			e.Properties = new EqArray<(string, string)>(props);
			e.RowRefs = new EqArray<(string, string)>(rowRefs);

			sheetPatchData.Add(e);
		}

		return sheetPatchData;
	}
}
