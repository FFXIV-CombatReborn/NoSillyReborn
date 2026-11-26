namespace NoSillyReborn.UI;

internal sealed partial class MainWindow
{
	private const float ScaleStep = 0.05f;
	private const float MotionStepPercent = 5f;

	private static readonly M3ColorChoice[] AccentPresets =
	[
		new("Lavender (default)", Configs.DefaultAccent),
		new("Ocean", M3ColorMath.FromRgb(0x2F6DB5)),
		new("Teal", M3ColorMath.FromRgb(0x00897B)),
		new("Forest", M3ColorMath.FromRgb(0x3E8E41)),
		new("Amber", M3ColorMath.FromRgb(0xC98A00)),
		new("Violet", M3ColorMath.FromRgb(0x7E57C2)),
		new("Crimson", M3.DefaultSeed),
	];

	private void DrawAppearancePage()
	{
		using var card = M3Card.Begin("appearance_theme", "Theme", FontAwesomeIcon.Palette,
			subtitle: "Applies to every No Silly Reborn window.");

		var config = Service.Config;

		var accent = config.UiAccentColor;
		if (M3Widgets.RowColorChoices("Accent color", ref accent, AccentPresets,
			"The whole color scheme is built from this one color. Pick a preset, or the last swatch for your own color."))
		{
			config.UiAccentColor = accent with { W = 1f };
			_savePending = true;
		}

		var text = config.UiTextScale;
		if (M3Widgets.RowDragFloat("Text size", ref text, 0.75f, 2f, "%.2fx",
			"Scales the text, on top of Dalamud's own font size.", step: ScaleStep))
		{
			config.UiTextScale = MathF.Round(text, 2);
			_savePending = true;
		}

		var element = config.UiElementScale;
		if (M3Widgets.RowDragFloat("Control size", ref element, 0.75f, 2f, "%.2fx",
			"Scales buttons, switches, fields and the other controls.", step: ScaleStep))
		{
			config.UiElementScale = MathF.Round(element, 2);
			_savePending = true;
		}

		var padding = config.UiPaddingScale;
		if (M3Widgets.RowDragFloat("Spacing", ref padding, 0.5f, 2f, "%.2fx",
			"Scales the space around and between things, without resizing them.", step: ScaleStep))
		{
			config.UiPaddingScale = MathF.Round(padding, 2);
			_savePending = true;
		}

		var motion = config.UiMotionScale * 100f;
		if (M3Widgets.RowDragFloat("Animation length", ref motion, 0f, 200f, "%.0f%%",
			"How long animations take, such as folding a window or opening a card. 0% turns them off.", step: MotionStepPercent))
		{
			config.UiMotionScale = MathF.Round(motion) / 100f;
			_savePending = true;
		}

		var compactLayout = config.UiCompactLayout;
		if (M3Widgets.RowSwitch("Compact layout", ref compactLayout,
			"Tighter padding and shorter rows, so more fits on screen."))
		{
			config.UiCompactLayout = compactLayout;
			_savePending = true;
		}

		var inlineDescriptions = config.UiInlineDescriptions;
		if (M3Widgets.RowSwitch("Inline descriptions", ref inlineDescriptions,
			"Show each setting's description under its name. Turn it off to show descriptions on hover instead."))
		{
			config.UiInlineDescriptions = inlineDescriptions;
			_savePending = true;
		}

		var iconSidebar = config.UiIconSidebar;
		if (M3Widgets.RowSwitch("Icons-only sidebar", ref iconSidebar,
			"Turn the sidebar into a narrow strip of icons. Hover an icon to see its name."))
		{
			config.UiIconSidebar = iconSidebar;
			_savePending = true;
		}

		var isDefault = config is { UiTextScale: 1f, UiElementScale: 1f, UiPaddingScale: 1f, UiMotionScale: 1f };
		if (M3Widgets.Button("##appearance_reset_sizes", "Reset sizes", M3ButtonStyle.Text, FontAwesomeIcon.Undo, enabled: !isDefault))
		{
			config.UiTextScale = 1f;
			config.UiElementScale = 1f;
			config.UiPaddingScale = 1f;
			config.UiMotionScale = 1f;
			_savePending = true;
		}
	}
}
