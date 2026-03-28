using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.DalamudServices;
using Lumina.Excel;
using NoSillyReborn.Windows;

namespace NoSillyReborn;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    private const string CommandName = "/nosilly";
    public readonly WindowSystem WindowSystem = new("No Silly Reborn");

    /// <summary>
    /// Instance-level configuration. Also exposed statically via <see cref="Config"/>
    /// so that the intercept manager and job rules can read it without holding a
    /// direct reference to the Plugin instance.
    /// </summary>
    public Configuration Configuration { get; init; }

    /// <summary>Static accessor for the active configuration.</summary>
    internal static Configuration Config { get; private set; } = null!;

    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    internal ActionInterceptManager InterceptManager { get; init; }

    public Plugin()
    {
        ECommonsMain.Init(PluginInterface, this, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.InitialiseDefaults();
        Config = Configuration;

        InterceptManager = new ActionInterceptManager(GameInteropProvider);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the No Silly Reborn window. Use /nosilly to toggle."
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Log.Information("[NoSillyReborn] Plugin loaded.");
    }

    public static ExcelSheet<T> GetSheet<T>() where T : struct, IExcelRow<T>
    {
        return Svc.Data.GetExcelSheet<T>(new ClientLanguage?(), null);
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        InterceptManager.Dispose();

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        ECommonsMain.Dispose();
        Log.Information("[NoSillyReborn] Plugin unloaded.");
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.Toggle();
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}


