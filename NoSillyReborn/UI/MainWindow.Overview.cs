namespace NoSillyReborn.UI;

internal sealed partial class MainWindow
{
	private void DrawOverviewPage()
	{
		DrawGuardCard();
		DrawCurrentJobCard();
		DrawSessionCard();
	}

	private static void DrawGuardCard()
	{
		using (M3Card.Begin("overview_guard", "Input guard", FontAwesomeIcon.ShieldAlt, style: M3CardStyle.Elevated))
		{
			var enabled = Service.Config.Enabled;
			if (M3Widgets.RowSwitch("Guard my inputs", ref enabled, "When this is off, every input goes through untouched."))
			{
				SetEnabled(enabled);
			}

			var holdToCast = Service.Config.HoldToCast;
			if (M3Widgets.RowSwitch("Hold a button to keep casting", ref holdToCast,
				$"Keeps casting while you hold a hotbar button down, instead of needing to mash it. {Service.AltCommand} hold does the same."))
			{
				SetHoldToCast(holdToCast);
			}

			var (message, severity, icon) = GuardState();
			ImGui.Dummy(new Vector2(0f, M3.Space1));
			_ = M3Widgets.Banner("##overview_state", message, severity, icon);
		}
	}

	private static (string Message, M3Severity Severity, FontAwesomeIcon Icon) GuardState()
	{
		if (!NoSillyRebornPlugin.InterceptorActive)
		{
			return ("The action hook isn't running, so nothing can be guarded. Check /xllog for the error.", M3Severity.Error, FontAwesomeIcon.Bug);
		}

		if (!Service.Config.Enabled)
		{
			return ("Paused. Every input goes through untouched.", M3Severity.Warning, FontAwesomeIcon.PauseCircle);
		}

		if (!DataCenter.PlayerAvailable())
		{
			return ("Waiting for your character to load.", M3Severity.Info, FontAwesomeIcon.HourglassHalf);
		}

		if (DataCenter.IsPvP)
		{
			return ("Paused in PvP. The rules and hold casting only cover PvE actions.", M3Severity.Info, FontAwesomeIcon.InfoCircle);
		}

		if (DataCenter.IsRsrAutorotationActive)
		{
			return ("Paused while Rotation Solver Reborn autorotation is active. Every input goes through untouched.", M3Severity.Warning, FontAwesomeIcon.PauseCircle);
		}

		var holdReady = Service.Config.HoldToCast && NoSillyRebornPlugin.HoldCasterActive;
		var job = DataCenter.Job;
		var name = job.GetName();
		if (RuleBook.For(job) is not { } set)
		{
			if (holdReady)
			{
				return Service.Config.OnlyInCombat && !DataCenter.InCombat
					? ("Waiting for combat. Hold casting is only active while you're in combat.", M3Severity.Info, FontAwesomeIcon.HourglassHalf)
					: ($"Hold casting is on. There's nothing else to guard for {name}.", M3Severity.Success, FontAwesomeIcon.CheckCircle);
			}

			return Service.Config.HoldToCast
				? ("Hold casting hooks aren't running. Check /xllog for the error.", M3Severity.Error, FontAwesomeIcon.Bug)
				: ($"There's nothing to guard for {name}.", M3Severity.Neutral, FontAwesomeIcon.InfoCircle);
		}

		if (!set.IsEnabled)
		{
			if (holdReady)
			{
				return Service.Config.OnlyInCombat && !DataCenter.InCombat
					? ("Waiting for combat. Hold casting is only active while you're in combat.", M3Severity.Info, FontAwesomeIcon.HourglassHalf)
					: ($"Rules are off for {name}, but hold casting is on.", M3Severity.Success, FontAwesomeIcon.CheckCircle);
			}

			if (Service.Config.HoldToCast && !NoSillyRebornPlugin.HoldCasterActive)
			{
				return ($"Turned off for {name}. Hold casting hooks aren't running; check /xllog.", M3Severity.Warning, FontAwesomeIcon.Bug);
			}

			return ($"Turned off for {name}.", M3Severity.Warning, FontAwesomeIcon.PauseCircle);
		}

		var active = set.EnabledCount;
		if (active == 0)
		{
			if (holdReady)
			{
				return Service.Config.OnlyInCombat && !DataCenter.InCombat
					? ("Waiting for combat. Hold casting is only active while you're in combat.", M3Severity.Info, FontAwesomeIcon.HourglassHalf)
					: ($"No rules are switched on for {name}, but hold casting is on.", M3Severity.Success, FontAwesomeIcon.CheckCircle);
			}

			if (Service.Config.HoldToCast && !NoSillyRebornPlugin.HoldCasterActive)
			{
				return ($"No rules are switched on for {name}. Hold casting hooks aren't running; check /xllog.", M3Severity.Warning, FontAwesomeIcon.Bug);
			}

			return ($"No rules are switched on for {name}.", M3Severity.Neutral, FontAwesomeIcon.InfoCircle);
		}

		if (Service.Config.OnlyInCombat && !DataCenter.InCombat)
		{
			return ("Waiting for combat. Inputs are only checked while you're in combat.", M3Severity.Info, FontAwesomeIcon.HourglassHalf);
		}

		if (Service.Config.HoldToCast && !NoSillyRebornPlugin.HoldCasterActive)
		{
			return ($"Guarding {Plural(active, "rule", "rules")} for {name}. Hold casting hooks aren't running; check /xllog.",
				M3Severity.Warning, FontAwesomeIcon.Bug);
		}

		return holdReady
			? ($"Guarding {Plural(active, "rule", "rules")} for {name} with hold casting on.", M3Severity.Success, FontAwesomeIcon.CheckCircle)
			: ($"Guarding {Plural(active, "rule", "rules")} for {name}.", M3Severity.Success, FontAwesomeIcon.CheckCircle);
	}

	private void DrawCurrentJobCard()
	{
		if (!DataCenter.PlayerAvailable())
		{
			return;
		}

		var job = DataCenter.Job;
		var rotationType = DataCenter.PlayerRotationType;
		using (M3Card.Begin("overview_job", "Your job", FontAwesomeIcon.UserShield, M3.Scheme.Tertiary))
		{
			if (RuleBook.For(job, rotationType) is not { } set)
			{
				M3Text.Draw($"No Silly Reborn has nothing to guard for {job.GetName()}.", M3TextStyle.Muted, true);
				return;
			}

			DrawJobHeader(set, job, $"Level {DataCenter.PlayerSyncedLevel()}");
			DrawRuleList(set);

			if (M3SettingRow.NavigationRow("##overview_role", $"Every {RoleName(set.Role)} rule", null,
				FontAwesomeIcon.ListUl, FontAwesomeIcon.ChevronRight, M3.Scheme.Tertiary))
			{
				_ = ShowPage(RolePageId(set.Role, rotationType));
			}
		}
	}

	private void DrawSessionCard()
	{
		using (M3Card.Begin("overview_session", "This session", FontAwesomeIcon.History, M3.Scheme.Secondary))
		{
			DrawCounters();

			if (InterceptLog.Latest is { } latest)
			{
				DrawRecordRow(latest);
			}
			else
			{
				M3Text.Draw("Nothing has been blocked or rerouted yet.", M3TextStyle.Muted, true);
			}

			if (M3SettingRow.NavigationRow("##overview_activity", "Open the activity log", null,
				FontAwesomeIcon.History, FontAwesomeIcon.ChevronRight, M3.Scheme.Secondary))
			{
				_ = ShowPage(ActivityPageId);
			}
		}
	}

	private static void DrawCounters()
	{
		var s = M3.Scheme;
		_ = M3Widgets.Pill("##counter_blocked", Plural(InterceptLog.Blocked, "block", "blocks"), s.Error, FontAwesomeIcon.Ban);
		ImGui.SameLine(0f, M3.Space1);
		_ = M3Widgets.Pill("##counter_rerouted", Plural(InterceptLog.Rerouted, "reroute", "reroutes"), s.Primary, FontAwesomeIcon.ExchangeAlt);
		ImGui.SameLine(0f, M3.Space1);
		_ = M3Widgets.Pill("##counter_since", $"since {InterceptLog.Since:t}", s.OnSurfaceVariant, FontAwesomeIcon.Clock);
		ImGui.Dummy(new Vector2(0f, M3.Space1));
	}
}
