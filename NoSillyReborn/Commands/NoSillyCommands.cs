using Dalamud.Game.Command;
using NoSillyReborn.UI;

namespace NoSillyReborn.Commands;

internal static class NoSillyCommands
{
	private const string Usage = "on, off, toggle, hold, rules or activity";

	public static void Enable()
	{
		_ = Svc.Commands.AddHandler(Service.Command, new CommandInfo(OnCommand)
		{
			HelpMessage = $"Open {Service.Name}. Add {Usage} to switch input guarding or jump to a page.",
			ShowInHelp = true,
		});
		_ = Svc.Commands.AddHandler(Service.AltCommand, new CommandInfo(OnCommand)
		{
			HelpMessage = $"Same as {Service.Command}.",
			ShowInHelp = true,
		});
	}

	public static void Disable()
	{
		_ = Svc.Commands.RemoveHandler(Service.Command);
		_ = Svc.Commands.RemoveHandler(Service.AltCommand);
	}

	private static void OnCommand(string command, string arguments)
	{
		var argument = (arguments ?? string.Empty).Trim();
		switch (argument.ToLowerInvariant())
		{
			case "":
				NoSillyRebornPlugin.ToggleMainWindow();
				break;

			case "on":
				SetEnabled(true);
				break;

			case "off":
				SetEnabled(false);
				break;

			case "toggle":
				SetEnabled(!Service.Config.Enabled);
				break;

			case "hold":
			case "hold toggle":
				SetHoldToCast(!Service.Config.HoldToCast);
				break;

			case "hold on":
				SetHoldToCast(true);
				break;

			case "hold off":
				SetHoldToCast(false);
				break;

			case "rules":
				var rotationType = DataCenter.PlayerRotationType;
				NoSillyRebornPlugin.OpenMainWindow(RuleBook.For(DataCenter.Job, rotationType) is { } set
					? MainWindow.RolePageId(set.Role, rotationType)
					: MainWindow.OverviewPageId);
				break;

			case "activity":
				NoSillyRebornPlugin.OpenMainWindow(MainWindow.ActivityPageId);
				break;

			default:
				Svc.Chat.PrintError($"Unknown option \"{argument}\". Use {Usage}.", Service.ChatTag);
				break;
		}
	}

	private static void SetEnabled(bool enabled)
	{
		Service.Config.Enabled = enabled;
		Service.Config.Save();
		Svc.Chat.Print(enabled ? "Input guarding is on." : "Input guarding is paused.", Service.ChatTag);
	}

	private static void SetHoldToCast(bool enabled)
	{
		Service.Config.HoldToCast = enabled;
		Service.Config.Save();
		Svc.Chat.Print(enabled ? "Press and hold casting is on." : "Press and hold casting is off.", Service.ChatTag);
	}
}
