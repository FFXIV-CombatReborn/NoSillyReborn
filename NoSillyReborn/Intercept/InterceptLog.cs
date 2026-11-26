namespace NoSillyReborn.Intercept;

internal static class InterceptLog
{
	private static readonly TimeSpan RepeatWindow = TimeSpan.FromSeconds(2);
	private static readonly List<InterceptRecord> _records = [];
	private static readonly Dictionary<string, int> _ruleHits = [];

	public static DateTime Since { get; private set; } = DateTime.Now;

	public static int Blocked { get; private set; }

	public static int Rerouted { get; private set; }

	public static int Unseen { get; private set; }

	public static int Count => _records.Count;

	public static InterceptRecord? Latest => _records.Count == 0 ? null : _records[^1];

	public static InterceptRecord Newest(int index)
	{
		return _records[_records.Count - 1 - index];
	}

	public static int HitsFor(GuardRule rule)
	{
		return _ruleHits.TryGetValue(rule.Key, out var hits) ? hits : 0;
	}

	public static bool Add(InterceptRecord record)
	{
		if (_records.Count > 0 && record.IsRepeatOf(_records[^1], RepeatWindow))
		{
			var last = _records[^1];
			_records[^1] = record with { Repeats = last.Repeats + 1 };
			return false;
		}

		_records.Add(record);
		Trim();

		if (record.Blocked)
		{
			Blocked++;
		}
		else
		{
			Rerouted++;
		}

		_ruleHits[record.Rule.Key] = HitsFor(record.Rule) + 1;
		Unseen++;
		return true;
	}

	public static void Trim()
	{
		var capacity = Math.Clamp(Service.Config.HistorySize, Configs.MinHistorySize, Configs.MaxHistorySize);
		if (_records.Count > capacity)
		{
			_records.RemoveRange(0, _records.Count - capacity);
		}
	}

	public static void MarkSeen()
	{
		Unseen = 0;
	}

	public static void Clear()
	{
		_records.Clear();
		_ruleHits.Clear();
		Blocked = 0;
		Rerouted = 0;
		Unseen = 0;
		Since = DateTime.Now;
	}
}
