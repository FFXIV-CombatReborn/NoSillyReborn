using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using NoSillyReborn.Commands;
using NoSillyReborn.Rules.Melee;
using NoSillyReborn.UI;

namespace NoSillyReborn;

public sealed class NoSillyRebornPlugin : IAsyncDalamudPlugin
{
	private static MainWindow? _mainWindow;
	private static ActionInterceptor? _interceptor;
	private static HoldCaster? _holdCaster;

	private readonly WindowSystem _windowSystem = new("NoSillyReborn");
	private bool _configLoaded;

	public NoSillyRebornPlugin(IDalamudPluginInterface pluginInterface)
	{
		ECommonsMain.Init(pluginInterface, this, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);

		M3.ElementBaseline = 0.75f;
		M3.Initialize(pluginInterface, Service.Config);
		M3Brand.Name = Service.Name;
		M3Brand.Version = Service.Version;
		M3Brand.FallbackIcon = FontAwesomeIcon.ShieldAlt;
		M3Brand.LogoCornerFraction = Service.LogoCornerFraction;
		M3Brand.SetLogoResource(typeof(NoSillyRebornPlugin).Assembly, Service.LogoResource);
	}

	internal static bool InterceptorActive => _interceptor?.IsActive == true;

	internal static bool HoldCasterActive => _holdCaster?.IsActive == true;

	public async Task LoadAsync(CancellationToken cancellationToken)
	{
		Service.Config = await Configs.LoadAsync(cancellationToken);
		_configLoaded = true;

		await Svc.Framework.Run(() =>
		{
			_mainWindow = new MainWindow();
			_windowSystem.AddWindow(_mainWindow);

			_interceptor = new ActionInterceptor();
			_holdCaster = new HoldCaster();
			NoSillyCommands.Enable();

			Svc.PluginInterface.UiBuilder.Draw += OnDraw;
			Svc.PluginInterface.UiBuilder.OpenMainUi += OnOpenMainUi;
			Svc.PluginInterface.UiBuilder.OpenConfigUi += OnOpenConfigUi;
		}, cancellationToken);
	}

	public async ValueTask DisposeAsync()
	{
		await Svc.Framework.Run(() =>
		{
			Svc.PluginInterface.UiBuilder.Draw -= OnDraw;
			Svc.PluginInterface.UiBuilder.OpenMainUi -= OnOpenMainUi;
			Svc.PluginInterface.UiBuilder.OpenConfigUi -= OnOpenConfigUi;

			NoSillyCommands.Disable();
			_holdCaster?.Dispose();
			_holdCaster = null;
			_interceptor?.Dispose();
			_interceptor = null;

			_windowSystem.RemoveAllWindows();
			_mainWindow = null;
		});

		RecentActions.Clear();
		InterceptFeedback.Clear();
		NinjaRules.Reset();

		if (_configLoaded)
		{
			Service.Config.Save();
		}

		M3.Dispose();
		RSR_IPCSubscriber.Dispose();
		ECommonsMain.Dispose();
	}

	internal static void ToggleMainWindow()
	{
		_mainWindow?.ToggleWindow();
	}

	internal static void OpenMainWindow(string? pageId = null)
	{
		_mainWindow?.Open(pageId);
	}

	private void OnDraw()
	{
		M3Window.DefaultDensity = Service.Config.UiCompactLayout ? M3Density.Tight : M3Density.Compact;
		M3.BeginFrame();
		_windowSystem.Draw();
	}

	private static void OnOpenMainUi()
	{
		OpenMainWindow(MainWindow.OverviewPageId);
	}

	private static void OnOpenConfigUi()
	{
		OpenMainWindow(MainWindow.BehaviorPageId);
	}
}
