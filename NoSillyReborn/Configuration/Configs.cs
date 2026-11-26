using Dalamud.Configuration;
using ECommons.Logging;
using Newtonsoft.Json.Linq;

namespace NoSillyReborn.Configuration;

internal sealed class Configs : IPluginConfiguration, IM3Settings
{
	public const int CurrentVersion = 2;

	public const int MinHistorySize = 10;
	public const int MaxHistorySize = 500;
	public const float MinHoldDelay = 0.1f;
	public const float MaxHoldDelay = 0.5f;

	public static readonly Vector4 DefaultAccent = M3ColorMath.FromRgb(0x847DA3);

	private static readonly Job[] LegacyJobs = [Job.AST, Job.BRD, Job.BLM, Job.DNC, Job.DRG, Job.MCH, Job.RDM, Job.SAM, Job.SMN, Job.WHM];

	public int Version { get; set; } = CurrentVersion;

	public bool Enabled { get; set; } = true;

	public bool OnlyInCombat { get; set; }

	public bool InterceptMacros { get; set; } = true;

	public bool HoldToCast { get; set; } = false;

	public bool HoldToCastAbilities { get; set; }

	public float HoldToCastDelay { get; set; } = 0.2f;

	public FeedbackMode BlockFeedback { get; set; } = FeedbackMode.Toast;

	public FeedbackMode ReplaceFeedback { get; set; } = FeedbackMode.Off;

	public float FeedbackCooldown { get; set; } = 2f;

	public int HistorySize { get; set; } = 100;

	public SortedSet<Job> DisabledJobs { get; set; } = [];

	public Dictionary<string, bool> RuleOverrides { get; set; } = [];

	public SortedSet<Job> DisabledEvolvedJobs { get; set; } = [];

	public Dictionary<string, bool> EvolvedRuleOverrides { get; set; } = [];

	public Vector4 UiAccentColor { get; set; } = DefaultAccent;

	public float UiTextScale { get; set; } = 1f;

	public float UiElementScale { get; set; } = 1f;

	public float UiPaddingScale { get; set; } = 1f;

	public float UiMotionScale { get; set; } = 1f;

	public bool UiCompactLayout { get; set; }

	public bool UiInlineDescriptions { get; set; } = true;

	public bool UiIconSidebar { get; set; }

	bool IM3Settings.UiDescriptionTooltips => !UiInlineDescriptions;

	public void Save()
	{
		try
		{
			var path = Svc.PluginInterface.ConfigFile.FullName;
			if (Path.GetDirectoryName(path) is { } directory)
			{
				Directory.CreateDirectory(directory);
			}

			var tempPath = path + ".tmp";
			File.WriteAllText(tempPath, JsonConvert.SerializeObject(this, Formatting.Indented));
			File.Move(tempPath, path, true);
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"[Configs] Failed to save the configuration: {ex.Message}");
		}
	}

	public SortedSet<Job> DisabledJobsFor(RotationType rotationType)
	{
		return rotationType == RotationType.Evolved ? DisabledEvolvedJobs : DisabledJobs;
	}

	public Dictionary<string, bool> RuleOverridesFor(RotationType rotationType)
	{
		return rotationType == RotationType.Evolved ? EvolvedRuleOverrides : RuleOverrides;
	}

	public void ResetRules()
	{
		DisabledJobs.Clear();
		DisabledEvolvedJobs.Clear();
		RuleOverrides.Clear();
		EvolvedRuleOverrides.Clear();
		Save();
	}

	public static async Task<Configs> LoadAsync(CancellationToken cancellationToken)
	{
		var file = Svc.PluginInterface.ConfigFile;
		if (!file.Exists)
		{
			return new Configs();
		}

		try
		{
			var json = JObject.Parse(await File.ReadAllTextAsync(file.FullName, cancellationToken));
			var version = json.Value<int?>(nameof(Version)) ?? 0;
			if (version < CurrentVersion)
			{
				var migrated = Migrate(json).Normalize();
				migrated.Save();
				return migrated;
			}

			return (json.ToObject<Configs>() ?? new Configs()).Normalize();
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			PluginLog.Warning($"[Configs] Failed to read the configuration, starting from the defaults: {ex.Message}");
			BackupUnreadable(file);
			return new Configs();
		}
	}

	private static Configs Migrate(JObject json)
	{
		var configs = new Configs();
		if (json.Value<bool?>("IsEnabled") is { } enabled)
		{
			configs.Enabled = enabled;
		}

		if (json["EnabledJobs"] is JArray { Count: > 0 } enabledJobs)
		{
			var kept = new HashSet<uint>();
			foreach (var token in enabledJobs)
			{
				if (token.Type == JTokenType.Integer)
				{
					var value = token.Value<long>();
					if (value is >= 0 and <= uint.MaxValue)
					{
						kept.Add((uint)value);
					}
				}
			}

			foreach (var job in LegacyJobs)
			{
				if (!kept.Contains((uint)job))
				{
					configs.DisabledJobs.Add(job);
				}
			}
		}

		return configs;
	}

	private Configs Normalize()
	{
		DisabledJobs ??= [];
		RuleOverrides ??= [];
		DisabledEvolvedJobs ??= [];
		EvolvedRuleOverrides ??= [];
		HoldToCastDelay = Math.Clamp(HoldToCastDelay, MinHoldDelay, MaxHoldDelay);
		FeedbackCooldown = Math.Clamp(FeedbackCooldown, 0f, 30f);
		HistorySize = Math.Clamp(HistorySize, MinHistorySize, MaxHistorySize);
		UiTextScale = Math.Clamp(UiTextScale, 0.75f, 2f);
		UiElementScale = Math.Clamp(UiElementScale, 0.75f, 2f);
		UiPaddingScale = Math.Clamp(UiPaddingScale, 0.5f, 2f);
		UiMotionScale = Math.Clamp(UiMotionScale, 0f, 2f);
		UiAccentColor = UiAccentColor with { W = 1f };
		return this;
	}

	private static void BackupUnreadable(FileInfo file)
	{
		try
		{
			if (file.DirectoryName != null)
			{
				File.Copy(file.FullName, Path.Combine(file.DirectoryName, "NoSillyReborn_LoadFailed.json"), true);
			}
		}
		catch (Exception ex)
		{
			PluginLog.Warning($"[Configs] Failed to back up the unreadable configuration: {ex.Message}");
		}
	}
}
