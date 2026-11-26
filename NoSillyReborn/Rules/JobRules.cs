using Dalamud.Game.ClientState.JobGauge.Types;
using ECommons.GameHelpers;

namespace NoSillyReborn.Rules;

internal abstract class JobRules
{
	private const float PendingWindow = 1.5f;
	private const float MitigationOverlapSeconds = 3f;

	private GuardRule[]? _rules;
	private Dictionary<uint, GuardRule[]>? _index;

	public abstract Job Job { get; }

	public RotationType RotationType { get; init; } = RotationType.Reborn;

	public virtual Job BaseClass => Job.ADV;

	public abstract JobRole Role { get; }

	public GuardRule[] Rules => _rules ??= RotationType == RotationType.Evolved ? DefineEvolved() : Define();

	public bool IsEnabled
	{
		get => !Service.Config.DisabledJobsFor(RotationType).Contains(Job);
		set
		{
			var disabledJobs = Service.Config.DisabledJobsFor(RotationType);
			if (value)
			{
				_ = disabledJobs.Remove(Job);
			}
			else
			{
				_ = disabledJobs.Add(Job);
			}
		}
	}

	public int EnabledCount
	{
		get
		{
			var count = 0;
			foreach (var rule in Rules)
			{
				if (rule.IsEnabled)
				{
					count++;
				}
			}
			return count;
		}
	}

	public ReadOnlySpan<GuardRule> RulesFor(ActionID actionId)
	{
		_index ??= BuildIndex();
		return _index.TryGetValue((uint)actionId, out var rules) ? rules : [];
	}

	protected abstract GuardRule[] Define();

	protected abstract GuardRule[] DefineEvolved();

	private Dictionary<uint, GuardRule[]> BuildIndex()
	{
		var grouped = new Dictionary<uint, List<GuardRule>>();
		foreach (var rule in Rules)
		{
			foreach (var trigger in rule.Triggers)
			{
				if (!grouped.TryGetValue((uint)trigger, out var list))
				{
					list = [];
					grouped[(uint)trigger] = list;
				}

				list.Add(rule);
			}
		}

		var index = new Dictionary<uint, GuardRule[]>(grouped.Count);
		foreach (var (actionId, list) in grouped)
		{
			index[actionId] = [.. list];
		}

		return index;
	}

	protected GuardRule Block(string id, string name, string description, string reason, ActionID trigger, Func<InterceptContext, bool> when,
		bool enabled = true)
	{
		return Block(id, name, description, reason, [trigger], when, enabled);
	}

	protected GuardRule Block(string id, string name, string description, string reason, ActionID[] triggers, Func<InterceptContext, bool> when,
		bool enabled = true)
	{
		return new GuardRule(Job, RotationType, id, name, description, reason, triggers, ActionID.None, ActionID.None, when, enabled);
	}

	protected GuardRule Replace(string id, string name, string description, string reason, ActionID[] triggers, ActionID replacement,
		ActionID preview, Func<InterceptContext, bool> when, bool enabled = true)
	{
		return new GuardRule(Job, RotationType, id, name, description, reason, triggers, replacement, preview, when, enabled);
	}

	protected GuardRule KeepCharge(ActionID action, string actionName, StatusID[] statuses, string? description = null)
	{
		return KeepCharge("keep-charge", action, actionName, statuses, description);
	}

	protected GuardRule KeepCharge(string id, ActionID action, string actionName, StatusID[] statuses, string? description = null)
	{
		return Block(id, $"Keep your second {actionName} charge",
			description ?? $"Blocks {actionName} while its effect is still running, so a double press doesn't burn both charges.",
			"it's already active", action, _ => Self(statuses) || JustUsed(action));
	}

	protected GuardRule ComboStep(string id, string name, string description, string reason, ActionID[] triggers, ActionID replacement,
		ActionID preview, params ActionID[] after)
	{
		return Replace(id, name, description, reason, triggers, replacement, preview, _ => ComboIs(after));
	}

	protected GuardRule PreserveCombo(string? description = null, Func<InterceptContext, bool>? when = null)
	{
		return PreserveCombo(GameMetadata.NonPreservingComboActions(Job), description, when);
	}

	protected GuardRule PreserveCombo(ActionID[] triggers, string? description = null, Func<InterceptContext, bool>? when = null)
	{
		return Block("preserve-combo", "Block combo-breaking actions",
			description ?? "Blocks actions that don't preserve combos while a combo is active, so a stray press doesn't break your chain. Off by default.",
			"it would break your current combo", triggers,
			context => context.Action.WouldBreakCombo() && (when == null || when(context)), false);
	}

	protected GuardRule KeepTargetCharge(string id, ActionID action, string actionName, StatusID[] statuses, string? description = null)
	{
		return Block(id, $"Keep your second {actionName} charge",
			description ?? $"Blocks {actionName} while its effect is already on your target, so a double press doesn't burn both charges on the same person.",
			"it's already active on that target", action, context => TargetHas(context, statuses) || JustUsedOnTarget(action, context));
	}

	protected GuardRule MitigationOverlap(ActionID action, string actionName)
	{
		return Block("mitigation-overlap", $"Don't overlap {actionName}",
			$"Blocks {actionName} while Troubadour, Tactician or Shield Samba from anyone has more than {MitigationOverlapSeconds:0} seconds left, since they don't stack.",
			"another ranged mitigation is still up", action,
			_ => StatusHelper.PlayerLongestStatusTime(false, StatusHelper.RangePhysicalDefense) > MitigationOverlapSeconds);
	}

	protected GuardRule DoubleRaise(ActionID raise, string actionName)
	{
		return Block("double-raise", "Don't double raise",
			$"Blocks {actionName} when your target already has a raise waiting, or someone else is already casting one on them.",
			"they're already being raised", raise, context => context.Target.IsBeingRaised());
	}

	protected GuardRule BlockRescue()
	{
		return Block("rescue", "Block Rescue",
			"Stops Rescue from going off at all, for when you never want to pull someone by accident.",
			"Rescue is switched off", ActionID.RescuePvE, _ => true, false);
	}

	protected GuardRule KeepStance(ActionID release, string stanceName)
	{
		return Block("keep-stance", $"Keep {stanceName} on in combat",
			$"Blocks Release {stanceName} during combat, so a stray press doesn't drop your tank stance. Off by default, since tank swaps need it.",
			"your tank stance stays on in combat", release, _ => InCombat, false);
	}

	protected static bool Self(params ReadOnlySpan<StatusID> statuses)
	{
		return StatusHelper.PlayerHasStatus(true, statuses);
	}

	protected static bool JustUsed(ActionID action)
	{
		return RecentActions.UsedWithin(action, PendingWindow);
	}

	protected static bool JustUsedOnTarget(ActionID action, in InterceptContext context)
	{
		var targetId = context.EffectiveTarget?.GameObjectId ?? Player.Object?.GameObjectId ?? 0;
		return RecentActions.UsedOnTargetWithin(action, targetId, PendingWindow);
	}

	protected static bool InCombat => DataCenter.InCombat;

	protected static T Gauge<T>() where T : JobGaugeBase
	{
		return Svc.Gauges.Get<T>();
	}

	protected static bool ComboIs(params ReadOnlySpan<ActionID> actions)
	{
		var current = DataCenter.ComboAction;
		if (current == ActionID.None)
		{
			return false;
		}

		for (var i = 0; i < actions.Length; i++)
		{
			if (actions[i] == current)
			{
				return true;
			}
		}

		return false;
	}

	protected static bool TargetHas(in InterceptContext context, params ReadOnlySpan<StatusID> statuses)
	{
		var target = context.EffectiveTarget as IBattleChara ?? Player.Object;
		return target != null && target.HasStatus(false, statuses);
	}
}
