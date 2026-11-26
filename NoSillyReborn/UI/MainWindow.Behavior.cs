namespace NoSillyReborn.UI;

internal sealed partial class MainWindow
{
	private static readonly M3Segment[] FeedbackSegments =
	[
		new("Off", FontAwesomeIcon.BellSlash, "No notice"),
		new("Toast", FontAwesomeIcon.CommentAlt, "A short notice on screen"),
		new("Chat", FontAwesomeIcon.Comments, "A line in your chat log"),
	];

	private void DrawBehaviorPage()
	{
		var config = Service.Config;

		using (M3Card.Begin("behavior_when", "When to step in", FontAwesomeIcon.ShieldAlt))
		{
			var enabled = config.Enabled;
			if (M3Widgets.RowSwitch("Guard my inputs", ref enabled, $"The master switch. {Service.AltCommand} toggle does the same."))
			{
				SetEnabled(enabled);
			}

			var combat = config.OnlyInCombat;
			if (M3Widgets.RowSwitch("Only in combat", ref combat, "Leaves your inputs alone while you're out of combat."))
			{
				config.OnlyInCombat = combat;
				config.Save();
			}

			var macros = config.InterceptMacros;
			if (M3Widgets.RowSwitch("Check macros too", ref macros, "Also checks actions used from macros, such as /ac lines."))
			{
				config.InterceptMacros = macros;
				config.Save();
			}
		}

		using (M3Card.Begin("behavior_hold", "Press and hold casting", FontAwesomeIcon.Repeat, M3.Scheme.Primary))
		{
			var holdToCast = config.HoldToCast;
			if (M3Widgets.RowSwitch("Hold a button to keep casting", ref holdToCast,
				$"Keeps casting while you hold a hotbar button down, instead of needing to mash it. {Service.AltCommand} hold does the same."))
			{
				SetHoldToCast(holdToCast);
			}

			var holdAbilities = config.HoldToCastAbilities;
			if (M3Widgets.RowSwitch("Repeat abilities too", ref holdAbilities,
				"Also repeats off-GCD abilities while held. Leave this off to only fire an ability once per hold so charges aren't spent back-to-back.",
				holdToCast))
			{
				config.HoldToCastAbilities = holdAbilities;
				config.Save();
			}

			var holdDelay = config.HoldToCastDelay;
			if (M3Widgets.RowDragFloat("Hold delay", ref holdDelay, Configs.MinHoldDelay, Configs.MaxHoldDelay, "%.2f s",
				"How long a button must be held before it starts repeating.", enabled: holdToCast, step: 0.05f))
			{
				config.HoldToCastDelay = holdDelay;
				_savePending = true;
			}
		}

		using (M3Card.Begin("behavior_feedback", "Telling you about it", FontAwesomeIcon.Bell, M3.Scheme.Tertiary))
		{
			var block = (int)config.BlockFeedback;
			if (M3Widgets.RowSegmented("When an input is blocked###feedback_block", ref block, FeedbackSegments,
				"A blocked button does nothing, so a notice tells you why."))
			{
				config.BlockFeedback = (FeedbackMode)block;
				config.Save();
				InterceptFeedback.Preview(true, config.BlockFeedback);
			}

			var replace = (int)config.ReplaceFeedback;
			if (M3Widgets.RowSegmented("When an input is rerouted###feedback_replace", ref replace, FeedbackSegments,
				"The action that fires shows on your hotbar anyway, so this starts off."))
			{
				config.ReplaceFeedback = (FeedbackMode)replace;
				config.Save();
				InterceptFeedback.Preview(false, config.ReplaceFeedback);
			}

			var cooldown = config.FeedbackCooldown;
			if (M3Widgets.RowDragFloat("Quiet time between notices", ref cooldown, 0f, 10f, "%.1f s",
				"Mashing a blocked button only shows one notice per rule in this time.", step: 0.5f))
			{
				config.FeedbackCooldown = cooldown;
				_savePending = true;
			}
		}

		using (M3Card.Begin("behavior_activity", "Activity log", FontAwesomeIcon.History, M3.Scheme.Secondary))
		{
			var size = config.HistorySize;
			if (M3Widgets.RowDragInt("Intercepts to keep", ref size, Configs.MinHistorySize, Configs.MaxHistorySize,
				"How many entries the activity log remembers this session.", step: 10))
			{
				config.HistorySize = size;
				InterceptLog.Trim();
				_savePending = true;
			}
		}

		using (M3Card.Begin("behavior_reset", "Reset", FontAwesomeIcon.Undo, M3.Scheme.Error))
		{
			if (M3Widgets.RowHoldButton("Reset every rule###reset_rules", "Hold to reset",
				"Turns every job back on and puts each rule back to its default.", 1f, M3ButtonStyle.Danger, FontAwesomeIcon.Undo))
			{
				config.ResetRules();
				M3Snackbar.Show("Every rule is back to its default.", severity: M3Severity.Success);
			}
		}
	}
}
