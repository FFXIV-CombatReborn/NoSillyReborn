using Dalamud.Game.Gui.Toast;

namespace NoSillyReborn.Intercept;

internal static class InterceptFeedback
{
	private static readonly Dictionary<string, long> _lastShown = [];

	public static void Notify(InterceptRecord record)
	{
		var config = Service.Config;
		var mode = record.Blocked ? config.BlockFeedback : config.ReplaceFeedback;
		if (mode == FeedbackMode.Off)
		{
			return;
		}

		var now = Environment.TickCount64;
		var cooldown = (long)(Math.Clamp(config.FeedbackCooldown, 0f, 30f) * 1000f);
		if (_lastShown.TryGetValue(record.Rule.Key, out var last) && now - last < cooldown)
		{
			return;
		}

		_lastShown[record.Rule.Key] = now;
		_ = Svc.Framework.RunOnTick(() => Show(record, mode));
	}

	public static void Preview(bool blocked, FeedbackMode mode)
	{
		if (mode == FeedbackMode.Off || RuleBook.Example(blocked) is not { } rule)
		{
			return;
		}

		var used = blocked ? ActionID.None : rule.Preview;
		Show(new InterceptRecord(DateTime.Now, rule.Job, rule.ShownTrigger, used, rule), mode);
	}

	public static void Clear()
	{
		_lastShown.Clear();
	}

	private static void Show(InterceptRecord record, FeedbackMode mode)
	{
		var message = record.Summary;
		switch (mode)
		{
			case FeedbackMode.Toast when record.Blocked:
				Svc.Toasts.ShowError(message);
				break;

			case FeedbackMode.Toast:
				Svc.Toasts.ShowQuest(message, new QuestToastOptions
				{
					IconId = record.Used.GetIcon(),
					PlaySound = false,
				});
				break;

			case FeedbackMode.Chat when record.Blocked:
				Svc.Chat.PrintError(message, Service.ChatTag);
				break;

			case FeedbackMode.Chat:
				Svc.Chat.Print(message, Service.ChatTag);
				break;
		}
	}
}
