namespace NoSillyReborn.UI;

internal sealed partial class MainWindow
{
	private readonly List<GuardRule> _searchResults = [];

	protected override bool SupportsSearch => true;

	protected override string SearchHint => "Search rules";

	private static void DrawRolePage(JobRole role, RotationType rotationType)
	{
		foreach (var set in RuleBook.AllFor(rotationType))
		{
			if (set.Role != role)
			{
				continue;
			}

			using (M3Card.Begin($"rules_{rotationType}_{set.Job}"))
			{
				DrawJobHeader(set, set.Job, null);
				DrawRuleList(set);
			}
		}
	}

	protected override void OnSearchChanged(string text)
	{
		_searchResults.Clear();
		var query = text.Trim();
		if (query.Length == 0)
		{
			return;
		}

		AddSearchResults(RuleBook.Reborn, query);
		AddSearchResults(RuleBook.Evolved, query);
	}

	protected override string SearchSubtitle(string text)
	{
		return _searchResults.Count == 1 ? "1 matching rule" : $"{_searchResults.Count} matching rules";
	}

	protected override void DrawSearchResults(string text)
	{
		if (_searchResults.Count == 0)
		{
			_ = M3Widgets.EmptyState("##search_empty", "No rules found",
				$"Nothing matches \"{text.Trim()}\". Try a job, an action or a shorter word.", FontAwesomeIcon.Search);
			return;
		}

		var accent = 0;
		DrawSearchGroup(RuleBook.Reborn, ref accent);
		DrawSearchGroup(RuleBook.Evolved, ref accent);
	}

	private void AddSearchResults(JobRules[] sets, string query)
	{
		foreach (var set in sets)
		{
			foreach (var rule in set.Rules)
			{
				if (rule.MatchesSearch(query))
				{
					_searchResults.Add(rule);
				}
			}
		}
	}

	private void DrawSearchGroup(JobRules[] sets, ref int accent)
	{
		foreach (var set in sets)
		{
			if (!HasResultFor(set))
			{
				continue;
			}

			var title = set.RotationType == RotationType.Evolved ? $"{set.Job.GetName()} (Evolved)" : set.Job.GetName();
			using (M3Card.Begin($"search_{set.RotationType}_{set.Job}", title, FontAwesomeIcon.None, M3.AccentFor(accent++), M3CardStyle.Outlined))
			{
				foreach (var rule in _searchResults)
				{
					if (rule.Job == set.Job && rule.RotationType == set.RotationType)
					{
						DrawRuleRow(rule, set.IsEnabled);
					}
				}

				if (M3SettingRow.NavigationRow($"##search_open_{set.RotationType}_{set.Job}", $"Every {RoleName(set.Role)} rule", null,
					FontAwesomeIcon.ListUl, FontAwesomeIcon.ChevronRight, M3.Scheme.Tertiary))
				{
					_ = ShowPage(RolePageId(set.Role, set.RotationType));
				}
			}
		}
	}

	private bool HasResultFor(JobRules set)
	{
		foreach (var rule in _searchResults)
		{
			if (rule.Job == set.Job && rule.RotationType == set.RotationType)
			{
				return true;
			}
		}

		return false;
	}
}
