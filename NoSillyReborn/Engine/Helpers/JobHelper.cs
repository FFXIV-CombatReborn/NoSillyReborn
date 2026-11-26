using System.Globalization;
using Lumina.Excel.Sheets;

namespace NoSillyReborn.Engine.Helpers;

internal static class JobHelper
{
	private const uint JobIconBase = 62100;

	private static readonly Dictionary<Job, string> _names = [];

	public static uint GetJobIcon(this Job job)
	{
		return JobIconBase + (uint)job;
	}

	public static string GetName(this Job job)
	{
		if (_names.TryGetValue(job, out var name))
		{
			return name;
		}

		name = Service.GetSheet<ClassJob>().TryGetRow((uint)job, out var row)
			? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(row.Name.ExtractText())
			: job.ToString();
		if (string.IsNullOrWhiteSpace(name))
		{
			name = job.ToString();
		}

		_names[job] = name;
		return name;
	}
}
