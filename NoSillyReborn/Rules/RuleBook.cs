using NoSillyReborn.Rules.Healer;
using NoSillyReborn.Rules.Magical;
using NoSillyReborn.Rules.Melee;
using NoSillyReborn.Rules.Ranged;
using NoSillyReborn.Rules.Tank;

namespace NoSillyReborn.Rules;

internal static class RuleBook
{
	public static JobRules[] Reborn { get; } = CreateRules(RotationType.Reborn);

	public static JobRules[] Evolved { get; } = CreateRules(RotationType.Evolved);

	public static JobRules[] All => Reborn;

	private static readonly Dictionary<Job, JobRules> _rebornByJob = BuildLookup(Reborn);
	private static readonly Dictionary<Job, JobRules> _evolvedByJob = BuildLookup(Evolved);

	public static int RuleCount
	{
		get
		{
			var count = 0;
			foreach (var set in Reborn)
			{
				count += set.Rules.Length;
			}
			foreach (var set in Evolved)
			{
				count += set.Rules.Length;
			}
			return count;
		}
	}

	public static JobRules[] AllFor(RotationType rotationType)
	{
		return rotationType == RotationType.Evolved ? Evolved : Reborn;
	}

	public static JobRules? For(Job job)
	{
		return For(job, DataCenter.PlayerRotationType);
	}

	public static JobRules? For(Job job, RotationType rotationType)
	{
		var lookup = rotationType == RotationType.Evolved ? _evolvedByJob : _rebornByJob;
		return lookup.TryGetValue(job, out var set) ? set : null;
	}

	public static GuardRule? Find(Job job, in InterceptContext context, Func<GuardRule, uint, bool>? canUse = null)
	{
		return Find(job, DataCenter.PlayerRotationType, context, canUse);
	}

	public static GuardRule? Find(Job job, RotationType rotationType, in InterceptContext context, Func<GuardRule, uint, bool>? canUse = null)
	{
		if (For(job, rotationType) is not { IsEnabled: true } set)
		{
			return null;
		}

		if (!context.Action.IsUnlocked())
		{
			return null;
		}

		foreach (var rule in set.RulesFor(context.Action))
		{
			if (!rule.IsEnabled || !rule.Matches(context))
			{
				continue;
			}

			if (rule.Blocks)
			{
				return rule;
			}

			var replacement = Service.GetAdjustedActionId((uint)rule.Replacement);
			if (replacement == (uint)context.Action || !((ActionID)replacement).IsUnlocked())
			{
				continue;
			}

			if (canUse != null && !canUse(rule, replacement))
			{
				continue;
			}

			return rule;
		}

		return null;
	}

	public static GuardRule? Example(bool blocks)
	{
		if (For(DataCenter.Job, DataCenter.PlayerRotationType) is { } currentSet)
		{
			foreach (var rule in currentSet.Rules)
			{
				if (rule.Blocks == blocks && rule.EnabledByDefault)
				{
					return rule;
				}
			}
		}

		foreach (var set in AllFor(DataCenter.PlayerRotationType))
		{
			foreach (var rule in set.Rules)
			{
				if (rule.Blocks == blocks && rule.EnabledByDefault)
				{
					return rule;
				}
			}
		}

		foreach (var set in Reborn)
		{
			foreach (var rule in set.Rules)
			{
				if (rule.Blocks == blocks && rule.EnabledByDefault)
				{
					return rule;
				}
			}
		}

		foreach (var set in Evolved)
		{
			foreach (var rule in set.Rules)
			{
				if (rule.Blocks == blocks && rule.EnabledByDefault)
				{
					return rule;
				}
			}
		}

		return null;
	}

	private static JobRules[] CreateRules(RotationType rotationType) =>
	[
		new PaladinRules { RotationType = rotationType },
		new WarriorRules { RotationType = rotationType },
		new DarkKnightRules { RotationType = rotationType },
		new GunbreakerRules { RotationType = rotationType },
		new WhiteMageRules { RotationType = rotationType },
		new ScholarRules { RotationType = rotationType },
		new AstrologianRules { RotationType = rotationType },
		new SageRules { RotationType = rotationType },
		new MonkRules { RotationType = rotationType },
		new DragoonRules { RotationType = rotationType },
		new NinjaRules { RotationType = rotationType },
		new SamuraiRules { RotationType = rotationType },
		new ReaperRules { RotationType = rotationType },
		new ViperRules { RotationType = rotationType },
		new BardRules { RotationType = rotationType },
		new MachinistRules { RotationType = rotationType },
		new DancerRules { RotationType = rotationType },
		new BlackMageRules { RotationType = rotationType },
		new SummonerRules { RotationType = rotationType },
		new RedMageRules { RotationType = rotationType },
		new PictomancerRules { RotationType = rotationType },
	];

	private static Dictionary<Job, JobRules> BuildLookup(JobRules[] rules)
	{
		var lookup = new Dictionary<Job, JobRules>();
		foreach (var set in rules)
		{
			lookup[set.Job] = set;
			if (set.BaseClass != Job.ADV)
			{
				lookup[set.BaseClass] = set;
			}
		}

		return lookup;
	}
}
