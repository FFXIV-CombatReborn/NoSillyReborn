using Dalamud.Interface.Utility.Raii;

namespace NoSillyReborn.UI;

internal sealed partial class MainWindow
{
	private const float JobIconSize = 36f;
	private const float RecordIconSize = 28f;

	private static float FlowIconSize => 28f * M3.Scale;

	private static void DrawJobHeader(JobRules set, Job shownJob, string? detail)
	{
		var enabled = set.IsEnabled;
		var summary = set.Rules.Length == 0
			? "No rules yet"
			: $"{set.EnabledCount} of {set.Rules.Length} rules on";
		if (detail != null)
		{
			summary = $"{detail} · {summary}";
		}

		M3RowInfo row;
		using (ImRaii.PushFont(M3.TitleMedium))
		{
			row = M3SettingRow.Begin(shownJob.GetName(), summary, M3Widgets.SwitchSize(),
				leadingTexture: M3ActionIcon.GameIcon(shownJob.GetJobIcon()), leadingTextureSize: JobIconSize);
		}

		ImGui.SetCursorScreenPos(row.ControlPosition);
		if (M3Widgets.Switch($"##job_{set.RotationType}_{set.Job}", ref enabled))
		{
			set.IsEnabled = enabled;
			Service.Config.Save();
		}

		M3SettingRow.End(row);
	}

	private static void DrawRuleList(JobRules set)
	{
		if (set.Rules.Length == 0)
		{
			M3Text.Draw("Nothing to guard for this job yet.", M3TextStyle.Muted, true);
			return;
		}

		using var group = M3SubGroup.Begin();
		foreach (var rule in set.Rules)
		{
			DrawRuleRow(rule, set.IsEnabled);
		}
	}

	private static void DrawRuleRow(GuardRule rule, bool jobEnabled)
	{
		var enabled = rule.IsEnabled;
		var switchSize = M3Widgets.SwitchSize();
		var flow = FlowSize(rule.Blocks);
		var gap = M3.Space3;
		var control = new Vector2(flow.X + gap + switchSize.X, MathF.Max(flow.Y, switchSize.Y));

		var hits = InterceptLog.HitsFor(rule);
		var supporting = hits == 0 ? rule.Description : $"{rule.Description} Stepped in {Times(hits)} this session.";

		var row = M3SettingRow.Begin(rule.Name, supporting, control, disabled: !jobEnabled);
		var origin = row.ControlPosition;
		DrawFlow(origin + new Vector2(0f, (control.Y - flow.Y) * 0.5f), rule.ShownTrigger, rule.Blocks ? ActionID.None : rule.Preview,
			jobEnabled && enabled);

		ImGui.SetCursorScreenPos(origin + new Vector2(flow.X + gap, (control.Y - switchSize.Y) * 0.5f));
		if (M3Widgets.Switch($"##rule_{rule.Key}", ref enabled, jobEnabled))
		{
			rule.IsEnabled = enabled;
			Service.Config.Save();
		}

		M3SettingRow.End(row);
	}

	private static void DrawRecordRow(InterceptRecord record)
	{
		var label = record.Blocked
			? $"Blocked {record.Pressed.GetName()}"
			: $"{record.Pressed.GetName()} → {record.Used.GetName()}";
		if (record.Repeats > 1)
		{
			label += $"  ×{record.Repeats}";
		}

		var supporting = $"{record.Time:T} · {record.Job.GetName()} · {record.Rule.Reason}";
		var flow = FlowSize(record.Blocked);
		var row = M3SettingRow.Begin(label, supporting, flow,
			leadingTexture: M3ActionIcon.GameIcon(record.Job.GetJobIcon()), leadingTextureSize: RecordIconSize);
		DrawFlow(row.ControlPosition, record.Pressed, record.Used, true);
		M3SettingRow.End(row);
	}

	private static void TextRow(string label, string? supporting)
	{
		var row = M3SettingRow.Begin(label, supporting, Vector2.Zero);
		M3SettingRow.End(row);
	}

	private static Vector2 FlowSize(bool blocks)
	{
		var icon = FlowIconSize;
		var glyph = M3Draw.MeasureIcon(blocks ? FontAwesomeIcon.Ban : FontAwesomeIcon.ArrowRight);
		var width = blocks
			? icon + M3.Space2 + glyph.X
			: icon + M3.Space2 + glyph.X + M3.Space2 + icon;
		return new Vector2(width, MathF.Max(icon, glyph.Y));
	}

	private static void DrawFlow(Vector2 origin, ActionID from, ActionID to, bool active)
	{
		var drawList = ImGui.GetWindowDrawList();
		var icon = FlowIconSize;
		var blocks = to == ActionID.None;
		var glyphIcon = blocks ? FontAwesomeIcon.Ban : FontAwesomeIcon.ArrowRight;
		var glyph = M3Draw.MeasureIcon(glyphIcon);
		var height = MathF.Max(icon, glyph.Y);
		var alpha = active ? 1f : M3.DisabledContent;

		var x = origin.X;
		DrawActionIcon(drawList, from, new Vector2(x, origin.Y + ((height - icon) * 0.5f)), icon, alpha);
		x += icon + M3.Space2;

		var tone = blocks ? M3.Scheme.Error : M3.Scheme.Primary;
		M3Draw.Icon(drawList, glyphIcon, new Vector2(x, origin.Y + ((height - glyph.Y) * 0.5f)), M3.Alpha(tone, alpha));

		if (!blocks)
		{
			x += glyph.X + M3.Space2;
			DrawActionIcon(drawList, to, new Vector2(x, origin.Y + ((height - icon) * 0.5f)), icon, alpha);
		}
	}

	private static void DrawActionIcon(ImDrawListPtr drawList, ActionID action, Vector2 min, float size, float alpha)
	{
		var max = min + new Vector2(size);
		var rounding = M3ActionIcon.Rounding(size);
		if (!M3ActionIcon.Image(drawList, M3ActionIcon.GameIcon(action.GetIcon()), min, max, rounding, alpha))
		{
			M3ActionIcon.EmptySlot(drawList, min, max, rounding);
		}

		if (ImGui.IsMouseHoveringRect(min, max) && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
		{
			M3Tooltip.Show(action.GetName());
		}
	}

	private static string Times(int count)
	{
		return count == 1 ? "once" : $"{count} times";
	}

	private static string Plural(int count, string singular, string plural)
	{
		return $"{count} {(count == 1 ? singular : plural)}";
	}
}
