using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NoSillyReborn.SourceGenerators;

/// <summary>
/// Roslyn incremental source generator that produces all FFXIV game-data enums
/// for NoSillyReborn from the embedded resource file.
///
/// Generated files (all in <c>NoSillyReborn.Data</c>):
/// <list type="bullet">
///   <item><c>StatusID.g.cs</c>   – every status buff/debuff ID</item>
///   <item><c>ActionID.g.cs</c>   – every action ID</item>
///   <item><c>ActionCate.g.cs</c> – action category (Weaponskill, Ability, Spell, …)</item>
///   <item><c>TerritoryContentType.g.cs</c> – content/duty category</item>
///   <item><c>OpCode.g.cs</c>     – network packet opcodes</item>
/// </list>
///
/// The embedded resource data (<c>Properties/Resources.resx</c>) is a verbatim
/// copy of the same file from RotationSolverReborn.  When the game patches and
/// RSR regenerates their resources, simply drop in the updated resx and rebuild.
///
/// NOTE: The <c>Action</c>, <c>DutyAction</c>, and <c>Rotation</c> resources in
/// the resx contain generated rotation base-class code that depends on
/// RSR.Basic types (<c>IBaseAction</c>, <c>CustomRotation</c>, etc.) which are
/// not present in NoSillyReborn.  Those resources are intentionally not emitted.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class StaticCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => s is ClassDeclarationSyntax,
            (c, _) => (ClassDeclarationSyntax)c.Node).Where(i => i is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, (spc, _) => Execute(spc));
    }

    private static void Execute(SourceProductionContext context)
    {
        try
        {
            GenerateStatusID(context);
            GenerateActionID(context);
            GenerateActionCate(context);
            GenerateContentType(context);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                "NSR0001",
                "Source Generation Error",
                $"An error occurred during source generation: {ex.Message}",
                "SourceGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true), Location.None));
        }
    }

    // ── Enum generators ───────────────────────────────────────────────────

    private static void GenerateStatusID(SourceProductionContext context)
    {
        var code = $$"""
            namespace NoSillyReborn.Data;

            /// <summary>
            /// FFXIV status (buff/debuff) IDs – generated from game data.
            /// Use <see cref="NoSillyReborn.Helpers.StatusHelper"/> to check whether the
            /// local player has a given status.
            /// </summary>
            public enum StatusID : ushort
            {
                /// <summary>No status.</summary>
                None = 0,
            {{Properties.Resources.StatusId.Table()}}
            }
            """;

        context.AddSource("StatusID.g.cs", code);
    }

    private static void GenerateActionID(SourceProductionContext context)
    {
        var code = $$"""
            namespace NoSillyReborn.Data;

            /// <summary>
            /// FFXIV action IDs – generated from game data.
            /// PvE actions follow the naming convention <c>&lt;Name&gt;PvE</c>
            /// (e.g. <see cref="BurstShotPvE"/>); PvP actions use <c>&lt;Name&gt;PvP</c>.
            /// </summary>
            public enum ActionID : uint
            {
                /// <summary>No action.</summary>
                None = 0,
            {{Properties.Resources.ActionId.Table()}}
            }
            """;

        context.AddSource("ActionID.g.cs", code);
    }

    private static void GenerateActionCate(SourceProductionContext context)
    {
        var code = $$"""
            namespace NoSillyReborn.Data;

            /// <summary>
            /// FFXIV action category (Weaponskill, Ability, Spell, etc.) – generated from game data.
            /// Returned by <see cref="Lumina.Excel.Sheets.Action.ActionCategory"/>.
            /// </summary>
            public enum ActionCate : byte
            {
                /// <summary>No category.</summary>
                None = 0,
            {{Properties.Resources.ActionCategory.Table()}}
            }
            """;

        context.AddSource("ActionCate.g.cs", code);
    }

    private static void GenerateContentType(SourceProductionContext context)
    {
        var code = $$"""
            namespace NoSillyReborn.Data;

            /// <summary>
            /// FFXIV territory content type (Dungeons, Raids, PvP, etc.) – generated from game data.
            /// Matches the <c>ContentType</c> column of the <c>TerritoryType</c> Excel sheet.
            /// </summary>
            public enum TerritoryContentType : byte
            {
                /// <summary>No content type.</summary>
                None = 0,
            {{Properties.Resources.ContentType.Table()}}
            }
            """;

        context.AddSource("TerritoryContentType.g.cs", code);
    }
}
