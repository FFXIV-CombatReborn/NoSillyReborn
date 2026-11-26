namespace NoSillyReborn.Intercept;

internal readonly record struct InterceptRecord(DateTime Time, Job Job, ActionID Pressed, ActionID Used, GuardRule Rule, int Repeats = 1)
{
	public bool Blocked => Used == ActionID.None;

	public string Summary => Blocked
		? $"{Pressed.GetName()} blocked: {Rule.Reason}"
		: $"{Pressed.GetName()} → {Used.GetName()}: {Rule.Reason}";

	public bool IsRepeatOf(in InterceptRecord other, TimeSpan window)
	{
		return Rule == other.Rule && Pressed == other.Pressed && Used == other.Used && Time - other.Time <= window;
	}
}
