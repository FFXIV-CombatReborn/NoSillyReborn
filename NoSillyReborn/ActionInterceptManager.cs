using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using NoSillyReborn.Jobs;

namespace NoSillyReborn;

/// <summary>
/// Hooks into <c>ActionManager.UseAction</c> to intercept player-initiated action
/// inputs and replace them with the correct action when a job rule matches.
///
/// Architecture summary:
///   1. The hook fires before the game processes the action.
///   2. We resolve the "adjusted" action ID (what the action really is at the
///      player's level, accounting for trait upgrades).
///   3. We look up the current job's <see cref="IJobRuleSet"/> from
///      <see cref="JobRuleRegistry"/> and evaluate each rule in order.
///   4. If a rule matches, we call the original UseAction with the replacement
///      action ID instead and return – the original action is never executed.
///   5. If no rule matches we forward the call unchanged.
/// </summary>
public sealed class ActionInterceptManager : IDisposable
{
    // ── Hook delegate ─────────────────────────────────────────────────────

    private unsafe delegate bool UseActionDelegate(
        ActionManager* actionManager,
        uint actionType,
        uint actionId,
        ulong targetObjectId,
        uint param,
        uint useType,
        int pvp,
        bool* isGroundTarget);

    private Hook<UseActionDelegate>? useActionHook;

    // ── State ─────────────────────────────────────────────────────────────

    /// <summary>The last action that was intercepted (used by the UI).</summary>
    public ActionIntercept? LastIntercept { get; private set; }

    // ── Constructor / Dispose ─────────────────────────────────────────────

    public unsafe ActionInterceptManager(IGameInteropProvider gameInterop)
    {
        try
        {
            useActionHook = gameInterop.HookFromAddress<UseActionDelegate>(
                ActionManager.Addresses.UseAction.Value,
                UseActionDetour);

            useActionHook.Enable();
            Plugin.Log.Information("[NoSillyReborn] Action intercept hook enabled.");
        }
        catch (Exception ex)
        {
            Plugin.Log.Error($"[NoSillyReborn] Failed to initialise action hook: {ex}");
        }
    }

    public void Dispose()
    {
        useActionHook?.Disable();
        useActionHook?.Dispose();
        useActionHook = null;
        Plugin.Log.Information("[NoSillyReborn] Action intercept hook disposed.");
    }

    // ── Hook detour ───────────────────────────────────────────────────────

    private unsafe bool UseActionDetour(
        ActionManager* actionManager,
        uint actionType,
        uint actionId,
        ulong targetObjectId,
        uint param,
        uint useType,
        int pvp,
        bool* isGroundTarget)
    {
        // Only intercept normal player actions (ActionType == 1).
        // Limit to in-combat inputs when the plugin is enabled.
        if (Plugin.Config.IsEnabled
            && actionType == 1
            && Player.Available)
        {
            try
            {
                var adjustedId = actionManager->GetAdjustedActionId(actionId);

                // Look up the rule-set for the player's current ClassJob.
                var jobId = Player.ClassJob.RowId;
                var ruleSet = JobRuleRegistry.GetRulesForJob(jobId);

                if (ruleSet != null && IsJobEnabled(jobId) && useActionHook != null)
                {
                    foreach (var rule in ruleSet.Rules)
                    {
                        if (rule.OriginalActionId != adjustedId) continue;

                        // Evaluate the condition (cheap – just status checks).
                        if (!rule.Condition()) continue;

                        if (rule.ReplacementActionId == 0)
                        {
                            // Rule matched – record and replace.
                            LastIntercept = new ActionIntercept(
                                adjustedId,
                                rule.ReplacementActionId,
                                rule.Description);
                            return false;
                        }

                        if (ActionManager.CanUseActionOnTarget(rule.ReplacementActionId, Svc.Targets.Target.Struct()))
                        {
                            // Rule matched – record and replace.
                            LastIntercept = new ActionIntercept(
                                adjustedId,
                                rule.ReplacementActionId,
                                rule.Description);
                            return useActionHook.Original(actionManager, actionType, rule.ReplacementActionId, targetObjectId, param, useType, pvp, isGroundTarget);
                        }

                        Plugin.Log.Debug(
                            $"[NoSillyReborn] Intercepted {adjustedId} → {rule.ReplacementActionId}: {rule.Description}");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"[NoSillyReborn] Error in UseActionDetour: {ex}");
            }
        }

        return useActionHook!.Original(
            actionManager, actionType, actionId, targetObjectId, param, useType, pvp, isGroundTarget);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static bool IsJobEnabled(uint jobId)
    {
        return Plugin.Config.EnabledJobs.Contains(jobId);
    }
}
