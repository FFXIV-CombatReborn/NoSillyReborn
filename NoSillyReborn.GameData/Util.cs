using System.Text;
using System.Text.RegularExpressions;
using Lumina.Excel.Sheets;

namespace NoSillyReborn.GameData;

internal static partial class Util
{
	/// <summary>
	/// Determines whether the specified <see cref="ClassJobCategory"/> represents a single combat job (and optional base class).
	/// </summary>
	/// <param name="jobCategory">The job category to check.</param>
	/// <returns><see langword="true"/> if the category represents a single combat job; otherwise, <see langword="false"/>.</returns>
	public static bool IsSingleJobForCombat(this ClassJobCategory jobCategory)
	{
		if (jobCategory.RowId == 68)
		{
			return true; // ACN SMN SCH
		}

		var str = jobCategory.Name.ToString().Replace(" ", "");
		if (str.Length is not 3 and not 6)
		{
			return false;
		}

		foreach (var c in str)
		{
			if (!char.IsUpper(c))
			{
				return false;
			}
		}

		return true;
	}
	/// <summary>
	/// Inserts spaces before each uppercase letter in the string.
	/// </summary>
	/// <param name="str">The string to modify.</param>
	/// <returns>The modified string with spaces.</returns>
	public static string Space(this string str)
	{
		var result = new StringBuilder();
		var lower = false;

		foreach (var c in str)
		{
			var isLower = char.IsLower(c);
			if (lower && !isLower)
			{
				result.Append(' ');
			}
			lower = isLower;
			result.Append(c);
		}

		return result.ToString();
	}

	/// <summary>
	/// Converts the string to PascalCase.
	/// </summary>
	/// <param name="input">The input string.</param>
	/// <returns>The PascalCase string.</returns>
	public static string ToPascalCase(this string input)
	{
		var cleaned = InvalidCharsRgx().Replace(WhiteSpace().Replace(input, "_"), string.Empty);
		var parts = cleaned.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
		var sb = new StringBuilder();
		foreach (var w0 in parts)
		{
			var w = StartWithLowerCaseChar().Replace(w0, m => m.Value.ToUpper());
			w = FirstCharFollowedByUpperCasesOnly().Replace(w, m => m.Value.ToLower());
			w = LowerCaseNextToNumber().Replace(w, m => m.Value.ToUpper());
			w = UpperCaseInside().Replace(w, m => m.Value.ToLower());
			sb.Append(w);
		}
		var result = sb.ToString();

		if (result.Length > 0 && char.IsNumber(result[0]))
		{
			result = "_" + result;
		}
		return result;
	}

	[GeneratedRegex("[^_a-zA-Z0-9]")]
	private static partial Regex InvalidCharsRgx();
	[GeneratedRegex("(?<=\\s)")]
	private static partial Regex WhiteSpace();
	[GeneratedRegex("^[a-z]")]
	private static partial Regex StartWithLowerCaseChar();
	[GeneratedRegex("(?<=[A-Z])[A-Z0-9]+$")]
	private static partial Regex FirstCharFollowedByUpperCasesOnly();
	[GeneratedRegex("(?<=[0-9])[a-z]")]
	private static partial Regex LowerCaseNextToNumber();
	[GeneratedRegex("(?<=[A-Z])[A-Z]+?((?=[A-Z][a-z])|(?=[0-9]))")]
	private static partial Regex UpperCaseInside();

	/// <summary>
	/// Sanitizes a description string to be safe for XML documentation comments.
	/// </summary>
	/// <param name="description">The description to sanitize.</param>
	/// <returns>The sanitized description safe for XML comments.</returns>
	public static string SanitizeXmlDescription(string description)
	{
		if (string.IsNullOrEmpty(description))
		{
			return description;
		}

		description = System.Text.RegularExpressions.Regex.Replace(description, @"<colortype\([^)]*\)>", "", RegexOptions.IgnoreCase);
		description = System.Text.RegularExpressions.Regex.Replace(description, @"</colortype>", "", RegexOptions.IgnoreCase);

		description = System.Text.RegularExpressions.Regex.Replace(description, @"<edgecolortype\([^)]*\)>", "", RegexOptions.IgnoreCase);
		description = System.Text.RegularExpressions.Regex.Replace(description, @"</edgecolortype>", "", RegexOptions.IgnoreCase);

		description = System.Text.RegularExpressions.Regex.Replace(description, @"<if\([^)]*\)>", "", RegexOptions.IgnoreCase);
		description = System.Text.RegularExpressions.Regex.Replace(description, @"</if>", "", RegexOptions.IgnoreCase);

		description = System.Text.RegularExpressions.Regex.Replace(description, @"</?br/?>", " ", RegexOptions.IgnoreCase);
		description = System.Text.RegularExpressions.Regex.Replace(description, @"</?indent/?>", "", RegexOptions.IgnoreCase);
		description = System.Text.RegularExpressions.Regex.Replace(description, @"</?sheet\([^)]*\)/?>", "", RegexOptions.IgnoreCase);
		description = System.Text.RegularExpressions.Regex.Replace(description, @"</?value\([^)]*\)/?>", "", RegexOptions.IgnoreCase);

		description = System.Text.RegularExpressions.Regex.Replace(description, @"<[^>]*\([^>]*\)[^>]*>", "", RegexOptions.IgnoreCase);

		description = System.Text.RegularExpressions.Regex.Replace(description, @"[,)]\s*>", "", RegexOptions.IgnoreCase);
		description = System.Text.RegularExpressions.Regex.Replace(description, @"<\s*[,)]", "", RegexOptions.IgnoreCase);

		description = System.Text.RegularExpressions.Regex.Replace(description, @"(?<=^|\s)[,)]+(?=\s|$)", "", RegexOptions.IgnoreCase);

		description = System.Text.RegularExpressions.Regex.Replace(description, @"\s+", " ");

		description = System.Text.RegularExpressions.Regex.Replace(description, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

		description = description.Replace("&", "&amp;");
		description = description.Replace("\"", "&quot;");

		return description.Trim();
	}

	/// <summary>
	/// Gets the description name for an action.
	/// </summary>
	/// <param name="action">The action.</param>
	/// <returns>The description name of the action.</returns>
	public static string GetDescName(this Lumina.Excel.Sheets.Action action)
	{
		var jobs = action.ClassJobCategory.IsValid ? action.ClassJobCategory.Value.Name.ToString() : string.Empty;
		jobs = string.IsNullOrEmpty(jobs) ? string.Empty : $" ({jobs})";

		var cate = (action.IsPvP || action.Unknown16) ? " <i>PvP</i>" : " <i>PvE</i>";

		return $"<see href=\"https://garlandtools.org/db/#action/{action.RowId}\"><strong>{action.Name}</strong></see>{cate}{jobs} [{action.RowId}] [{(action.ActionCategory.IsValid ? action.ActionCategory.Value.Name.ToString() : string.Empty)}]";
	}
}
