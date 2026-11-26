using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using ECommons.Logging;

namespace NoSillyReborn.UI;

internal sealed partial class MainWindow
{
	private const float HeroLogoSize = 56f;

	private void DrawAboutPage()
	{
		var s = M3.Scheme;

		using (M3Card.Begin("about_hero", null, style: M3CardStyle.Elevated))
		{
			DrawHeroTitle();
			ImGui.Dummy(new Vector2(0f, M3.Space2));
			M3Text.Draw("No Silly Reborn checks each action you use against a set of rules for your current job. " +
				"A rule either reroutes the press to the action that your buffs have made available, like Refulgent Arrow while Hawk's Eye is up, " +
				"or blocks a press that would be wasted, like a second Lightspeed charge while the buff from the first is still up. " +
				"It doesn't use actions on its own.", M3TextStyle.Muted, true);

			ImGui.Dummy(new Vector2(0f, M3.Space2));
			_ = M3Widgets.Pill("##about_version", $"v{Service.Version}", s.Primary, FontAwesomeIcon.CodeBranch);
			ImGui.SameLine(0f, M3.Space1);
			_ = M3Widgets.Pill("##about_rules", Plural(RuleBook.RuleCount, "rule", "rules"), s.Tertiary, FontAwesomeIcon.ShieldAlt);
			ImGui.SameLine(0f, M3.Space1);
			_ = M3Widgets.Pill("##about_team", "The Combat Reborn Team", s.Secondary, FontAwesomeIcon.Users);

			ImGui.Dummy(new Vector2(0f, M3.Space2));
			if (M3Widgets.Button("##about_repo", "Source code", M3ButtonStyle.Tonal, FontAwesomeIcon.CodeBranch))
			{
				OpenLink(Service.RepoUrl);
			}

			ImGui.SameLine(0f, M3.Space2);
			if (M3Widgets.Button("##about_discord", "Discord", M3ButtonStyle.Outlined, FontAwesomeIcon.Comments))
			{
				OpenLink(Service.DiscordUrl);
			}

			ImGui.SameLine(0f, M3.Space2);
			if (M3Widgets.Button("##about_kofi", "Ko-fi", M3ButtonStyle.Outlined, FontAwesomeIcon.MugHot))
			{
				OpenLink(Service.KofiUrl);
			}
		}

		using (M3Card.Begin("about_commands", "Commands", FontAwesomeIcon.Terminal, s.Tertiary))
		{
			TextRow(Service.Command, $"Opens or closes this window. {Service.AltCommand} does the same.");
			TextRow($"{Service.AltCommand} on · off · toggle", "Turns input guarding on, off, or flips it. Handy in a macro.");
			TextRow($"{Service.AltCommand} hold", "Turns press and hold casting on or off.");
			TextRow($"{Service.AltCommand} rules", "Opens the rules for the job you're playing.");
			TextRow($"{Service.AltCommand} activity", "Opens the activity log.");
		}

		using (M3Card.Begin("about_how", "How it works", FontAwesomeIcon.Route, s.Secondary))
		{
			M3Text.Bullet("Every action you use passes through the game's action manager, and No Silly Reborn looks at it there first.");
			M3Text.Bullet("A reroute only happens when the game says the replacement can be used right now. Otherwise your press goes through as it was.");
			M3Text.Bullet("A block swallows the press, so nothing is queued and nothing is spent.");
			M3Text.Bullet("When press and hold casting is on, holding a hotbar keybind or controller button keeps queuing the slot until you let go.");
			M3Text.Bullet("It stays out of PvP, and it only ever changes the button you pressed.");
		}
	}

	private static void DrawHeroTitle()
	{
		var logoSize = HeroLogoSize * M3.Scale;
		var origin = ImGui.GetCursorScreenPos();
		M3Brand.DrawLogo(ImGui.GetWindowDrawList(), origin, logoSize);

		using (ImRaii.PushFont(M3.TitleLarge))
		using (ImRaii.PushColor(ImGuiCol.Text, M3.Scheme.Primary))
		{
			var titleTop = origin.Y + ((logoSize - ImGui.GetTextLineHeight()) * 0.5f);
			ImGui.SetCursorScreenPos(new Vector2(origin.X + logoSize + M3.Space3, titleTop));
			ImGui.TextUnformatted(Service.Name);
		}

		ImGui.SetCursorScreenPos(new Vector2(origin.X, origin.Y + logoSize));
		ImGui.Dummy(Vector2.Zero);
	}

	private static void OpenLink(string url)
	{
		try
		{
			Util.OpenLink(url);
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"[MainWindow] Failed to open {url}: {ex.Message}");
		}
	}
}
