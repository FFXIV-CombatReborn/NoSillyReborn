using ECommons.Logging;

namespace NoSillyReborn.Rules;

internal sealed class GuardRule(Job job, RotationType rotationType, string id, string name, string description, string reason, ActionID[] triggers,
	ActionID replacement, ActionID preview, Func<InterceptContext, bool> condition, bool enabledByDefault)
{
	private readonly Func<InterceptContext, bool> _condition = condition;
	private bool _faulted;

	public Job Job { get; } = job;

	public RotationType RotationType { get; } = rotationType;

	public string Id { get; } = id;

	public string OverrideKey { get; } = $"{job}.{id}";

	public string Key { get; } = rotationType == RotationType.Evolved ? $"Evolved.{job}.{id}" : $"{job}.{id}";

	public string Name { get; } = name;

	public string Description { get; } = description;

	public string Reason { get; } = reason;

	public ActionID[] Triggers { get; } = triggers;

	public ActionID Replacement { get; } = replacement;

	public ActionID Preview { get; } = preview;

	public bool EnabledByDefault { get; } = enabledByDefault;

	public bool Blocks => Replacement == ActionID.None;

	public ActionID ShownTrigger => Triggers[^1];

	public bool IsEnabled
	{
		get => Service.Config.RuleOverridesFor(RotationType).TryGetValue(OverrideKey, out var enabled) ? enabled : EnabledByDefault;
		set
		{
			var overrides = Service.Config.RuleOverridesFor(RotationType);
			if (value == EnabledByDefault)
			{
				_ = overrides.Remove(OverrideKey);
			}
			else
			{
				overrides[OverrideKey] = value;
			}
		}
	}

	public bool Matches(in InterceptContext context)
	{
		try
		{
			return _condition(context);
		}
		catch (Exception ex)
		{
			if (!_faulted)
			{
				_faulted = true;
				PluginLog.Error($"[GuardRule] {Key} failed while checking its condition: {ex}");
			}

			return false;
		}
	}

	public bool MatchesSearch(string text)
	{
		if (Name.Contains(text, StringComparison.OrdinalIgnoreCase)
			|| Description.Contains(text, StringComparison.OrdinalIgnoreCase)
			|| Job.GetName().Contains(text, StringComparison.OrdinalIgnoreCase)
			|| Job.ToString().Contains(text, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		foreach (var trigger in Triggers)
		{
			if (trigger.GetName().Contains(text, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return !Blocks
			&& (Preview.GetName().Contains(text, StringComparison.OrdinalIgnoreCase)
				|| Replacement.GetName().Contains(text, StringComparison.OrdinalIgnoreCase));
	}
}
