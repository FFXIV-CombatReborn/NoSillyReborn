using Dalamud.Interface.Windowing;

namespace NoSillyReborn.UI;

internal sealed partial class MainWindow : M3ShellWindow
{
	public const string OverviewPageId = "overview";
	public const string ActivityPageId = "activity";
	public const string BehaviorPageId = "behavior";
	public const string AppearancePageId = "appearance";
	public const string AboutPageId = "about";

	private const string SectionGuard = "Guard";
	private const string SectionRulesReborn = "Rules - Reborn";
	private const string SectionRulesEvolved = "Rules - Evolved";
	private const string SectionSettings = "Settings";

	private static readonly Vector2 DefaultSize = new(920f, 660f);

	private static readonly WindowSizeConstraints DefaultConstraints = new()
	{
		MinimumSize = new Vector2(460f, 380f),
		MaximumSize = new Vector2(4000f, 4000f),
	};

	private static readonly M3WindowAction DiscordAction = new("##nsr_discord", FontAwesomeIcon.Comments, "Join the Discord");

	private static readonly M3WindowAction[] PauseActions =
	[
		DiscordAction,
		new("##nsr_pause", FontAwesomeIcon.Pause, "Pause input guarding"),
	];

	private static readonly M3WindowAction[] ResumeActions =
	[
		DiscordAction,
		new("##nsr_resume", FontAwesomeIcon.Play, "Resume input guarding"),
	];

	private static readonly RolePage[] RolePages =
	[
		new(JobRole.Tank, "tanks", "evolved_tanks", "Tanks", "tank", FontAwesomeIcon.ShieldAlt, "Paladin, Warrior, Dark Knight and Gunbreaker"),
		new(JobRole.Healer, "healers", "evolved_healers", "Healers", "healer", FontAwesomeIcon.Heartbeat, "White Mage, Scholar, Astrologian and Sage"),
		new(JobRole.Melee, "melee", "evolved_melee", "Melee", "melee", FontAwesomeIcon.FistRaised, "Monk, Dragoon, Ninja, Samurai, Reaper and Viper"),
		new(JobRole.RangedPhysical, "ranged", "evolved_ranged", "Physical Ranged", "physical ranged", FontAwesomeIcon.Crosshairs, "Bard, Machinist and Dancer"),
		new(JobRole.RangedMagical, "casters", "evolved_casters", "Magical Ranged", "magical ranged", FontAwesomeIcon.HatWizard, "Black Mage, Summoner, Red Mage and Pictomancer"),
	];

	private bool _savePending;

	public MainWindow()
		: base($"{Service.Name}###NoSillyRebornMain", "nsr", Service.Name, "NSR", DefaultSize, DefaultConstraints)
	{
		AllowPinning = false;
		AllowClickthrough = false;
		AddPage(new M3Page(OverviewPageId, "Overview", FontAwesomeIcon.Home, SectionGuard,
			"What No Silly Reborn is doing right now", DrawOverviewPage));
		AddPage(new M3Page(ActivityPageId, "Activity", FontAwesomeIcon.History, SectionGuard,
			"Every input No Silly Reborn changed this session", DrawActivityPage, badge: ActivityBadge));

		foreach (var page in RolePages)
		{
			var role = page.Role;
			AddPage(new M3Page(page.RebornId, page.Label, page.Icon, SectionRulesReborn, page.Description, () => DrawRolePage(role, RotationType.Reborn)));
		}

		foreach (var page in RolePages)
		{
			var role = page.Role;
			AddPage(new M3Page(page.EvolvedId, page.Label, page.Icon, SectionRulesEvolved, page.Description, () => DrawRolePage(role, RotationType.Evolved)));
		}

		AddPage(new M3Page(BehaviorPageId, "Behavior", FontAwesomeIcon.SlidersH, SectionSettings,
			"When No Silly Reborn steps in and how it tells you", DrawBehaviorPage));
		AddPage(new M3Page(AppearancePageId, "Appearance", FontAwesomeIcon.Palette, SectionSettings,
			"Accent color, sizes, spacing, animations and layout", DrawAppearancePage));
		AddPage(new M3Page(AboutPageId, "About", FontAwesomeIcon.InfoCircle, SectionSettings,
			"Commands, links and how it works", DrawAboutPage));
	}

	protected override M3WindowAction[] WindowActions => Service.Config.Enabled ? PauseActions : ResumeActions;

	public static string RolePageId(JobRole role, RotationType rotationType = RotationType.Reborn)
	{
		foreach (var page in RolePages)
		{
			if (page.Role == role)
			{
				return rotationType == RotationType.Evolved ? page.EvolvedId : page.RebornId;
			}
		}

		return OverviewPageId;
	}

	private static string RoleName(JobRole role)
	{
		foreach (var page in RolePages)
		{
			if (page.Role == role)
			{
				return page.Noun;
			}
		}

		return "job";
	}

	public void ToggleWindow()
	{
		if (IsOpen && IsMinimized)
		{
			Restore();
			BringToFront();
			return;
		}

		IsOpen = !IsOpen;
	}

	public void Open(string? pageId)
	{
		if (pageId != null)
		{
			_ = ShowPage(pageId);
		}

		OpenAndRestore();
	}

	protected override void PreDrawFold()
	{
		if (IsPinned || IsClickthrough)
		{
			IsPinned = false;
			IsClickthrough = false;
		}
	}

	protected override void OnWindowAction(int index)
	{
		switch (index)
		{
			case 0:
				OpenLink(Service.DiscordUrl);
				break;
			case 1:
				SetEnabled(!Service.Config.Enabled);
				break;
		}
	}

	protected override void DrawLast()
	{
		if (_savePending && !ImGui.IsAnyItemActive())
		{
			FlushSave();
		}
	}

	public override void OnClose()
	{
		FlushSave();
		base.OnClose();
	}

	private void FlushSave()
	{
		if (!_savePending)
		{
			return;
		}

		Service.Config.Save();
		_savePending = false;
	}

	private static string? ActivityBadge()
	{
		return InterceptLog.Unseen > 0 ? M3Widgets.BadgeCount(InterceptLog.Unseen) : null;
	}

	private static void SetEnabled(bool enabled)
	{
		Service.Config.Enabled = enabled;
		Service.Config.Save();
		M3Snackbar.Show(enabled ? "Input guarding is on." : "Input guarding is paused.",
			severity: enabled ? M3Severity.Success : M3Severity.Warning);
	}

	private static void SetHoldToCast(bool enabled)
	{
		Service.Config.HoldToCast = enabled;
		Service.Config.Save();
		M3Snackbar.Show(enabled ? "Press and hold casting is on." : "Press and hold casting is off.",
			severity: enabled ? M3Severity.Success : M3Severity.Warning);
	}

	private readonly record struct RolePage(JobRole Role, string RebornId, string EvolvedId, string Label, string Noun, FontAwesomeIcon Icon, string Description);
}
