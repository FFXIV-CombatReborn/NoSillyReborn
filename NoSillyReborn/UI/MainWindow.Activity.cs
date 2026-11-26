using Dalamud.Interface.Utility.Raii;

namespace NoSillyReborn.UI;

internal sealed partial class MainWindow
{
	private const int BusiestRuleCount = 5;

	private readonly List<GuardRule> _busiestRules = [];

	private void DrawActivityPage()
	{
		InterceptLog.MarkSeen();

		using (M3Card.Begin("activity_session", "This session", FontAwesomeIcon.ChartBar))
		{
			DrawCounters();
			DrawBusiestRules();

			if (M3Widgets.RowButton("Clear the activity log###activity_clear", "Clear", "Forgets every intercept and resets the counters.",
				M3ButtonStyle.Outlined, FontAwesomeIcon.TrashAlt, InterceptLog.Count > 0))
			{
				InterceptLog.Clear();
				M3Snackbar.Show("Activity cleared.");
			}
		}

		using (M3Card.Begin("activity_log", "Recent intercepts", FontAwesomeIcon.List, M3.Scheme.Tertiary))
		{
			if (InterceptLog.Count == 0)
			{
				_ = M3Widgets.EmptyState("##activity_empty", "Nothing intercepted yet",
					"When No Silly Reborn blocks or reroutes an input, it shows up here.", FontAwesomeIcon.ShieldAlt);
				return;
			}

			for (var i = 0; i < InterceptLog.Count; i++)
			{
				using (ImRaii.PushId(i))
				{
					DrawRecordRow(InterceptLog.Newest(i));
				}
			}
		}
	}

	private void DrawBusiestRules()
	{
		_busiestRules.Clear();
		CollectBusiestRules(RuleBook.Reborn);
		CollectBusiestRules(RuleBook.Evolved);

		if (_busiestRules.Count == 0)
		{
			return;
		}

		_busiestRules.Sort(static (a, b) => InterceptLog.HitsFor(b).CompareTo(InterceptLog.HitsFor(a)));

		M3Widgets.SectionLabel("Busiest rules");
		var shown = Math.Min(_busiestRules.Count, BusiestRuleCount);
		for (var i = 0; i < shown; i++)
		{
			var rule = _busiestRules[i];
			var jobLabel = rule.RotationType == RotationType.Evolved ? $"{rule.Job.GetName()} (Evolved)" : rule.Job.GetName();
			M3Widgets.RowValue($"{rule.Name}###busiest_{rule.Key}", Times(InterceptLog.HitsFor(rule)), jobLabel);
		}
	}

	private void CollectBusiestRules(JobRules[] sets)
	{
		foreach (var set in sets)
		{
			foreach (var rule in set.Rules)
			{
				if (InterceptLog.HitsFor(rule) > 0)
				{
					_busiestRules.Add(rule);
				}
			}
		}
	}
}
