using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;

namespace NoSillyReborn.Engine;

internal static class Service
{
	public const string Name = "No Silly Reborn";
	public const string Command = "/nosilly";
	public const string AltCommand = "/nsr";
	public const string ChatTag = "NoSilly";
	public const string RepoUrl = "https://github.com/FFXIV-CombatReborn/NoSillyReborn";
	public const string DiscordUrl = "https://discord.gg/p54TZMPnC9";
	public const string KofiUrl = "https://ko-fi.com/ltscombatreborn";
	public const string LogoResource = "NoSillyReborn.Resources.NSR_Icon.png";
	public const float LogoCornerFraction = 0.04f;

	private static Configs _config = new();

	public static string Version { get; } = typeof(Service).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";

	public static Configs Config
	{
		get => _config;
		set
		{
			_config = value;
			M3.Settings = value;
		}
	}

	public static ExcelSheet<T> GetSheet<T>() where T : struct, IExcelRow<T>
	{
		return Svc.Data.GetExcelSheet<T>()!;
	}

	public static unsafe uint GetAdjustedActionId(uint actionId)
	{
		var actionManager = ActionManager.Instance();
		return actionManager == null ? actionId : actionManager->GetAdjustedActionId(actionId);
	}
}
