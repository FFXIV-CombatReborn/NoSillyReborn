using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using NoSillyReborn.Jobs;

namespace NoSillyReborn.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("No Silly Reborn Configuration###NoSillyRebornConfig")
    {
        Flags = ImGuiWindowFlags.NoCollapse;
        Size = new Vector2(480, 360);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
        configuration = plugin.Configuration;
    }

    public void Dispose() => GC.SuppressFinalize(this);

    public override void PreDraw()
    {
        Flags = configuration.IsConfigWindowMovable
            ? Flags & ~ImGuiWindowFlags.NoMove
            : Flags | ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        // ── Master toggle ─────────────────────────────────────────────────
        var enabled = configuration.IsEnabled;
        if (ImGui.Checkbox("Enable No Silly Reborn (intercept wrong actions)", ref enabled))
        {
            configuration.IsEnabled = enabled;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // ── Window behaviour ──────────────────────────────────────────────
        var movable = configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable config window", ref movable))
        {
            configuration.IsConfigWindowMovable = movable;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // ── Per-job toggles ───────────────────────────────────────────────
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Per-Job Intercept Toggle");
        ImGui.Spacing();

        foreach (var ruleSet in JobRuleRegistry.AllRuleSets)
        {
            var jobId = (uint)ruleSet.JobId;
            var jobEnabled = configuration.EnabledJobs.Contains(jobId);

            if (ImGui.Checkbox($"{ruleSet.JobId}##job_{jobId}", ref jobEnabled))
            {
                if (jobEnabled)
                    configuration.EnabledJobs.Add(jobId);
                else
                    configuration.EnabledJobs.Remove(jobId);

                configuration.Save();
            }

            ImGui.SameLine();
            ImGui.TextDisabled($"({ruleSet.Rules.Count} rule{(ruleSet.Rules.Count == 1 ? "" : "s")})");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // ── Rule listing ──────────────────────────────────────────────────
        ImGui.TextColored(ImGuiColors.DalamudViolet, "Active Rules");
        ImGui.Spacing();

        foreach (var ruleSet in JobRuleRegistry.AllRuleSets)
        {
            if (!configuration.EnabledJobs.Contains((uint)ruleSet.JobId)) continue;

            if (ImGui.CollapsingHeader($"{ruleSet.JobId}##rules_{ruleSet.JobId}"))
            {
                ImGui.Indent();
                foreach (var rule in ruleSet.Rules)
                {
                    ImGui.BulletText(rule.Description);
                }
                ImGui.Unindent();
            }
        }
    }
}

