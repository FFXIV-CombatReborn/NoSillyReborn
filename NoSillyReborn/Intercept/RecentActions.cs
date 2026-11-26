namespace NoSillyReborn.Intercept;

internal static class RecentActions
{
	private readonly record struct Entry(long Tick, ulong TargetId);

	private static readonly Dictionary<uint, Entry> _lastUsed = [];

	public static void Record(uint actionId, ulong targetId = 0)
	{
		_lastUsed[actionId] = new Entry(Environment.TickCount64, targetId);
	}

	public static bool UsedWithin(ActionID actionId, float seconds)
	{
		return _lastUsed.TryGetValue((uint)actionId, out var entry) && Environment.TickCount64 - entry.Tick <= (long)(seconds * 1000f);
	}

	public static bool UsedOnTargetWithin(ActionID actionId, ulong targetId, float seconds)
	{
		return _lastUsed.TryGetValue((uint)actionId, out var entry)
			&& Environment.TickCount64 - entry.Tick <= (long)(seconds * 1000f)
			&& (targetId == 0 || entry.TargetId == 0 || entry.TargetId == targetId);
	}

	public static void Clear()
	{
		_lastUsed.Clear();
	}
}
