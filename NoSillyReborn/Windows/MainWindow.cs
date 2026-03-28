using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using NoSillyReborn.Jobs;


namespace NoSillyReborn.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("No Silly Reborn##main", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() => GC.SuppressFinalize(this);

    public override void Draw()
    {
        // ── Header ────────────────────────────────────────────────────────
        var enabledText = Plugin.Config.IsEnabled ? "Enabled" : "Disabled";
        var enabledColor = Plugin.Config.IsEnabled ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed;
        ImGui.TextColored(enabledColor, $"Status: {enabledText}");
        ImGui.SameLine();
        if (ImGui.SmallButton("Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // ── Current player info ────────────────────────────────────────────
        var localPlayer = Player.Object;
        if (localPlayer == null)
        {
            ImGui.TextDisabled("Not logged in or between zones.");
            return;
        }

        var rowId = (Job)Player.ClassJob.RowId;
        if (rowId == Job.ADV)
        {
            ImGui.TextDisabled("Loading job data...");
        }
        var jobName = rowId.ToString();
        var jobIconId = 62100u + (uint)rowId;
        var iconTexture = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(jobIconId)).GetWrapOrEmpty();

        ImGui.Text("Current job:");
        ImGui.SameLine(130 * ImGuiHelpers.GlobalScale);
        ImGui.Image(iconTexture.Handle, new Vector2(22, 22) * ImGuiHelpers.GlobalScale);
        ImGui.SameLine();
        ImGui.Text($"{jobName} [Level {localPlayer.Level}]");

        // Show whether this job has active rules
        var ruleSet = JobRuleRegistry.GetRulesForJob((uint)rowId);
        if (ruleSet != null && Plugin.Config.EnabledJobs.Contains((uint)ruleSet.JobId))
        {
            ImGui.TextColored(ImGuiColors.HealerGreen,
                $"  {ruleSet.Rules.Count} intercept rule{(ruleSet.Rules.Count == 1 ? "" : "s")} active.");
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudGrey, "  No intercept rules for this job.");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // ── Last intercepted action ────────────────────────────────────────
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Last Intercepted Action");
        ImGui.Spacing();

        var lastIntercept = plugin.InterceptManager.LastIntercept;
        if (lastIntercept == null)
        {
            ImGui.TextDisabled("No intercepts recorded yet.");
        }
        else
        {
            ImGui.Text($"Original ID : {lastIntercept.OriginalActionId}");
            ImGui.Text($"Replaced by : {lastIntercept.ReplacementActionId}");
            ImGui.TextWrapped($"Reason      : {lastIntercept.Description}");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // ── Active rules for current job ───────────────────────────────────
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Active Rules for Current Job");
        ImGui.Spacing();

        if (ruleSet == null)
        {
            ImGui.TextDisabled("No rules registered for this job.");
        }
        else
        {
            foreach (var rule in ruleSet.Rules)
            {
                ImGui.BulletText(rule.Description);
            }
        }
    }
}

