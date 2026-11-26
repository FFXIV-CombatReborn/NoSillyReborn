using Microsoft.CodeAnalysis;

namespace NoSillyReborn.SourceGenerators;

/// <summary>
/// A source generator that generates static code for various enums.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class StaticCodeGenerator : IIncrementalGenerator
{
	/// <summary>
	/// Initializes the generator with the provided context.
	/// </summary>
	/// <param name="context">The initialization context.</param>
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(Execute);
	}

	private static void Execute(IncrementalGeneratorPostInitializationContext context)
	{
		try
		{
			GenerateStatus(context);
			GenerateActionID(context);
			GenerateContentType(context);
			GenerateActionCate(context);
			GenerateBNpcNameID(context);
			GenerateOpCode(context);
			GenerateTraitID(context);
			GenerateGameMetadata(context);
		}
		catch (Exception ex)
		{
			context.AddSource("SourceGenerationError.g.cs", $"#error Source generation failed: {ex.Message.Replace('\r', ' ').Replace('\n', ' ')}");
		}
	}

	private static void GenerateOpCode(IncrementalGeneratorPostInitializationContext context)
	{
		var code = $$"""
			namespace NoSillyReborn.Engine.Data;

			/// <summary>
			/// The opcode
			/// </summary>
			public enum OpCode : ushort
			{
				/// <summary>
				///
				/// </summary>
				None = 0,
			{{Properties.OpCode.Value.Table()}}
			}
			""";

		context.AddSource("OpCode.g.cs", code);
	}

	private static void GenerateStatus(IncrementalGeneratorPostInitializationContext context)
	{
		var code = $$"""
			namespace NoSillyReborn.Engine.Data;

			/// <summary>
			/// The id of the status.
			/// </summary>
			public enum StatusID : ushort
			{
				/// <summary>
				///
				/// </summary>
				None = 0,
			{{Properties.Status.Value.Table()}}
			}
			""";

		context.AddSource("StatusID.g.cs", code);
	}

	private static void GenerateContentType(IncrementalGeneratorPostInitializationContext context)
	{
		var code = $$"""
			namespace NoSillyReborn.Engine.Data;

			/// <summary>
			///
			/// </summary>
			public enum TerritoryContentType : byte
			{
				/// <summary>
				///
				/// </summary>
				None = 0,
			{{Properties.ContentType.Value.Table()}}
			}
			""";

		context.AddSource("TerritoryContentType.g.cs", code);
	}

	private static void GenerateActionCate(IncrementalGeneratorPostInitializationContext context)
	{
		var code = $$"""
			namespace NoSillyReborn.Engine.Data;

			/// <summary>
			///
			/// </summary>
			public enum ActionCate : byte
			{
				/// <summary>
				///
				/// </summary>
				None = 0,
			{{Properties.ActionCategory.Value.Table()}}
			}
			""";

		context.AddSource("ActionCate.g.cs", code);
	}

	private static void GenerateBNpcNameID(IncrementalGeneratorPostInitializationContext context)
	{
		var code = $$"""
			namespace NoSillyReborn.Engine.Data;

			/// <summary>
			/// The id of the BNpc name.
			/// </summary>
			public enum NPCName : uint
			{
				/// <summary>
				///
				/// </summary>
				None = 0,
			{{Properties.BNpcName.Value.Table()}}
			}
			""";

		context.AddSource("BNpcNameID.g.cs", code);
	}

	private static void GenerateActionID(IncrementalGeneratorPostInitializationContext context)
	{
		var code = $$"""
			namespace NoSillyReborn.Engine.Data;

			/// <summary>
			/// The id of the status.
			/// </summary>
			public enum ActionID : uint
			{
				/// <summary>
				///
				/// </summary>
				None = 0,
			{{Properties.ActionId.Value.Table()}}
			}
			""";

		context.AddSource("ActionID.g.cs", code);
	}

	private static void GenerateTraitID(IncrementalGeneratorPostInitializationContext context)
	{
		var code = $$"""
			namespace NoSillyReborn.Engine.Data;

			/// <summary>
			/// The id of the combat trait.
			/// </summary>
			public enum TraitID : uint
			{
				/// <summary>
				///
				/// </summary>
				None = 0,
			{{Properties.TraitId.Value.Table()}}
			}
			""";

		context.AddSource("TraitID.g.cs", code);
	}

	private static void GenerateGameMetadata(IncrementalGeneratorPostInitializationContext context)
	{
		context.AddSource("GameMetadata.g.cs", Properties.GameMetadata.Value);
	}
}
