using Lumina.Excel.Sheets;

namespace NoSillyReborn.GameData.Getters;

internal class TraitIdGetter(Lumina.GameData gameData)
	: ExcelRowGetter<Trait>(gameData)
{
	private readonly HashSet<string> _addedNames = [];
	private readonly HashSet<uint> _nonCombatJobs = [];

	/// <summary>
	/// Called before creating the list of items. Populates non-combat job IDs and clears added names.
	/// </summary>
	protected override void BeforeCreating()
	{
		_addedNames.Clear();
		_nonCombatJobs.Clear();

		var classJobs = _gameData.GetExcelSheet<ClassJob>()!;
		foreach (var c in classJobs)
		{
			if (c.ClassJobCategory.RowId is 32 or 33)
			{
				_nonCombatJobs.Add(c.RowId);
			}
		}

		base.BeforeCreating();
	}

	/// <summary>
	/// Determines whether the specified trait should be added to the list.
	/// </summary>
	/// <param name="item">The trait to check.</param>
	/// <returns>True if the trait should be added; otherwise, false.</returns>
	protected override bool AddToList(Trait item)
	{
		if (item.ClassJob.RowId == 0 || !item.ClassJob.IsValid || _nonCombatJobs.Contains(item.ClassJob.RowId))
		{
			return false;
		}

		if (item.Icon == 0)
		{
			return false;
		}

		var name = item.Name.ToString();
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}

		foreach (var c in name)
		{
			if (!char.IsAscii(c))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Converts the specified trait to its enum member representation.
	/// </summary>
	/// <param name="item">The trait to convert.</param>
	/// <returns>The code representation of the trait.</returns>
	protected override string ToCode(Trait item)
	{
		var rawName = item.Name.ToString();
		var name = rawName.ToPascalCase();

		if (!_addedNames.Add(name))
		{
			name += "_" + item.RowId.ToString();
		}

		var jobAbbr = item.ClassJob.Value.Abbreviation.ToString();
		var desc = GetDesc(item);

		return $"""
        /// <summary>
        /// <see href="https://garlandtools.org/db/#action/{50000 + item.RowId}"><strong>{rawName.Replace("&", "and")}</strong></see> ({jobAbbr} Lv.{item.Level}) [{item.RowId}]
        /// {desc}
        /// </summary>
        {name} = {item.RowId},
        """;
	}

	private string GetDesc(Trait item)
	{
		var transient = _gameData.GetExcelSheet<TraitTransient>()?.GetRow(item.RowId);
		var desc = transient?.Description.ToString() ?? string.Empty;
		desc = Util.SanitizeXmlDescription(desc);
		return $"<para>{desc.Replace("\n", "</para>\n/// <para>")}</para>";
	}
}
