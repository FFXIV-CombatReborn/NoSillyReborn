using ECommons.EzIpcManager;
using ECommons.Logging;
using ECommons.Reflection;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace NoSillyReborn.Integrations.IPC;

internal static class RSR_IPCSubscriber
{
	private const string PluginInternalName = "RotationSolver";
	private const string IpcPrefix = "RotationSolverReborn";
	private const long ReadyCacheMs = 5000;
	private const long StateCacheMs = 50;

	private static EzIPCDisposalToken[]? _disposalTokens;
	private static bool _initialized;

	[EzIPC("AutorotationActive", true)]
	internal static readonly Func<bool>? AutorotationActive;

	private static bool _readyCached;
	private static long _readyCheckedAt = long.MinValue;

	private static bool _activeCached;
	private static long _activeCheckedAt = long.MinValue;

	internal static bool IsEnabled
	{
		get
		{
			var now = Environment.TickCount64;
			if (_readyCheckedAt != long.MinValue && now - _readyCheckedAt < ReadyCacheMs)
			{
				return _readyCached;
			}

			_readyCached = DalamudReflector.TryGetDalamudPlugin(PluginInternalName, out _, false, true);
			_readyCheckedAt = now;
			return _readyCached;
		}
	}

	internal static bool IsAutorotationActive()
	{
		if (!IsEnabled)
		{
			_activeCached = false;
			_activeCheckedAt = long.MinValue;
			return false;
		}

		var now = Environment.TickCount64;
		if (_activeCheckedAt != long.MinValue && now - _activeCheckedAt < StateCacheMs)
		{
			return _activeCached;
		}

		EnsureInitialized();
		_activeCached = AutorotationActive?.Invoke() == true;
		_activeCheckedAt = now;
		return _activeCached;
	}

	internal static void Dispose()
	{
		if (_disposalTokens != null)
		{
			foreach (var token in _disposalTokens)
			{
				try
				{
					token.Dispose();
				}
				catch (Exception ex)
				{
					PluginLog.Error($"[RSR_IPCSubscriber] Error while unregistering IPC: {ex}");
				}
			}

			_disposalTokens = null;
		}

		_initialized = false;
		_readyCached = false;
		_readyCheckedAt = long.MinValue;
		_activeCached = false;
		_activeCheckedAt = long.MinValue;
	}

	private static void EnsureInitialized()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;
		_disposalTokens = EzIPC.Init(typeof(RSR_IPCSubscriber), IpcPrefix, SafeWrapper.AnyException);
	}
}
