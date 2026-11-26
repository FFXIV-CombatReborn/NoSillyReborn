using Dalamud.Hooking;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using NoSillyReborn.Rules.Melee;

namespace NoSillyReborn.Intercept;

internal sealed unsafe class ActionInterceptor : IDisposable
{
	private static readonly ActionStatus[] UnusableStatuses =
	[
		ActionStatus.NotEnoughHp,
		ActionStatus.NotEnoughMp,
		ActionStatus.RequirementNotMet,
		ActionStatus.NotLearned,
		ActionStatus.WrongClass,
		ActionStatus.CannotExecute,
		ActionStatus.StatusAffliction,
		ActionStatus.NotUsableHere,
		ActionStatus.OnlyInPvP,
		ActionStatus.NoDutyActionCharges,
		ActionStatus.NotInActiveSet,
	];

	private readonly Hook<ActionManager.Delegates.UseAction>? _useActionHook;

	public ActionInterceptor()
	{
		try
		{
			_useActionHook = Svc.Hook.HookFromAddress<ActionManager.Delegates.UseAction>(ActionManager.Addresses.UseAction.Value, UseActionDetour);
			_useActionHook.Enable();
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[ActionInterceptor] Failed to hook UseAction: {ex}");
		}
	}

	public bool IsActive => _useActionHook is { IsEnabled: true, IsDisposed: false };

	public void Dispose()
	{
		_useActionHook?.Disable();
		_useActionHook?.Dispose();
	}

	private bool UseActionDetour(ActionManager* actionManager, ActionType actionType, uint actionId, ulong targetId, uint extraParam,
		ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted)
	{
		var verdict = actionType == ActionType.Action ? Inspect(actionManager, actionId, targetId, mode) : default;
		if (verdict.Rule == null)
		{
			return Use(actionManager, actionType, actionId, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);
		}

		if (verdict.Rule.Blocks)
		{
			if (!HoldCaster.IsRepeating)
			{
				Report(verdict, ActionID.None);
			}

			return false;
		}

		var used = Use(actionManager, actionType, verdict.Replacement, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);
		if (used)
		{
			Report(verdict, (ActionID)verdict.Replacement);
		}

		return used;
	}

	private static Verdict Inspect(ActionManager* actionManager, uint actionId, ulong targetId, ActionManager.UseActionMode mode)
	{
		try
		{
			if (!ShouldInspect(mode))
			{
				return default;
			}

			var job = DataCenter.Job;
			var pressed = actionManager->GetAdjustedActionId(actionId);
			if (RuleBook.Find(job, new InterceptContext((ActionID)pressed, targetId, mode),
					(_, replacement) => CanUse(actionManager, replacement, targetId)) is not { } rule)
			{
				return default;
			}

			if (rule.Blocks)
			{
				return new Verdict(rule, job, pressed, 0);
			}

			var replacement = actionManager->GetAdjustedActionId((uint)rule.Replacement);
			return replacement != pressed
				? new Verdict(rule, job, pressed, replacement)
				: default;
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[ActionInterceptor] Failed to inspect action {actionId}: {ex}");
			return default;
		}
	}

	private bool Use(ActionManager* actionManager, ActionType actionType, uint actionId, ulong targetId, uint extraParam,
		ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted)
	{
		var adjustedId = actionType == ActionType.Action ? actionManager->GetAdjustedActionId(actionId) : 0u;
		var effectiveTargetId = actionType == ActionType.Action
			? TargetHelper.ResolveForAction((ActionID)adjustedId, targetId)?.GameObjectId ?? 0UL
			: 0UL;
		var sequence = actionManager->LastUsedActionSequence;
		var result = _useActionHook!.Original(actionManager, actionType, actionId, targetId, extraParam, mode, comboRouteId, outOptAreaTargeted);

		if (actionType == ActionType.Action)
		{
			if (actionManager->LastUsedActionSequence != sequence)
			{
				RecentActions.Record(adjustedId, effectiveTargetId);
				NinjaRules.RecordExecutedAction((ActionID)adjustedId);
			}

			if (result && (mode == ActionManager.UseActionMode.Queue || actionManager->LastUsedActionSequence != sequence || !actionManager->ActionQueued))
			{
				HoldCaster.NotifyActionExecuted(adjustedId);
			}
		}

		return result;
	}

	private static bool ShouldInspect(ActionManager.UseActionMode mode)
	{
		var config = Service.Config;
		if (!config.Enabled || !DataCenter.PlayerAvailable() || DataCenter.IsPvP || DataCenter.IsRsrAutorotationActive)
		{
			return false;
		}

		if (mode == ActionManager.UseActionMode.Macro && !config.InterceptMacros)
		{
			return false;
		}

		return !config.OnlyInCombat || DataCenter.InCombat;
	}

	internal static bool CanUse(ActionManager* actionManager, uint actionId, ulong targetId)
	{
		var status = (ActionStatus)actionManager->GetActionStatus(ActionType.Action, actionId, targetId, false, false);
		if (Array.IndexOf(UnusableStatuses, status) >= 0)
		{
			return false;
		}

		return ActionHelper.IsRealGCD(actionId)
			|| actionManager->GetActionStatus(ActionType.Action, actionId, targetId, true, false) != (uint)ActionStatus.OnCooldown;
	}

	private static void Report(in Verdict verdict, ActionID used)
	{
		try
		{
			var record = new InterceptRecord(DateTime.Now, verdict.Job, (ActionID)verdict.Pressed, used, verdict.Rule!);
			if (InterceptLog.Add(record))
			{
				PluginLog.Debug($"[ActionInterceptor] {record.Summary} ({record.Rule.Key})");
			}

			InterceptFeedback.Notify(record);
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[ActionInterceptor] Failed to report an intercept: {ex}");
		}
	}

	private readonly record struct Verdict(GuardRule? Rule, Job Job, uint Pressed, uint Replacement);
}
