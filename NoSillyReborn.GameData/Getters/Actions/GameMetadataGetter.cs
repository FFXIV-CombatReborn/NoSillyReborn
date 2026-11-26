using System.Text;
using Lumina.Excel.Sheets;
using Action = Lumina.Excel.Sheets.Action;

namespace NoSillyReborn.GameData.Getters.Actions;

internal class GameMetadataGetter(Lumina.GameData gameData)
{
	private static readonly string[] CombatJobAbbreviations =
	[
		"PLD", "WAR", "DRK", "GNB",
		"WHM", "SCH", "AST", "SGE",
		"MNK", "DRG", "NIN", "SAM", "RPR", "VPR",
		"BRD", "MCH", "DNC",
		"BLM", "SMN", "RDM", "PCT",
	];

	private readonly Lumina.GameData _gameData = gameData ?? throw new ArgumentNullException(nameof(gameData));

	public string GetCode()
	{
		var actions = _gameData.GetExcelSheet<Action>()!;
		var traits = _gameData.GetExcelSheet<Trait>()!;
		var replaceActions = _gameData.GetSubrowExcelSheet<ReplaceAction>()!;
		var indirections = _gameData.GetExcelSheet<ActionIndirection>()!;
		var classJobs = _gameData.GetExcelSheet<ClassJob>()!;

		var actionNames = BuildActionNameMap(_gameData);
		var traitNames = BuildTraitNameMap(traits, classJobs);

		var replacedByPlayer = BuildReplacedByPlayerSet(replaceActions, indirections);
		var nonPreservingByJob = BuildNonPreservingByJob(actions, replacedByPlayer, actionNames);
		var comboStepNormalization = BuildComboStepNormalization(actions, replaceActions, indirections, actionNames);
		var actionUnlockLinks = BuildActionUnlockLinks(actions, traits, replaceActions);
		var actionLevels = BuildActionLevels(actions, actionNames);
		var traitLevels = BuildTraitLevels(traits, traitNames);
		var traitQuests = BuildTraitQuests(traits, traitNames);

		var sb = new StringBuilder();
		sb.AppendLine("using System;");
		sb.AppendLine("using ECommons.ExcelServices;");
		sb.AppendLine();
		sb.AppendLine("namespace NoSillyReborn.Engine.Data;");
		sb.AppendLine();
		sb.AppendLine("internal static class GameMetadata");
		sb.AppendLine("{");

		// Per-job non-preserving combo actions
		foreach (var abbr in CombatJobAbbreviations)
		{
			var list = nonPreservingByJob[abbr];
			sb.AppendLine($"\tprivate static readonly ActionID[] _nonPreserving{abbr} =");
			sb.AppendLine("\t[");
			foreach (var name in list)
			{
				sb.AppendLine($"\t\tActionID.{name},");
			}
			sb.AppendLine("\t];");
			sb.AppendLine();
		}

		sb.AppendLine("\tpublic static ActionID[] NonPreservingComboActions(Job job) => job switch");
		sb.AppendLine("\t{");
		sb.AppendLine("\t\tJob.GLA or Job.PLD => _nonPreservingPLD,");
		sb.AppendLine("\t\tJob.MRD or Job.WAR => _nonPreservingWAR,");
		sb.AppendLine("\t\tJob.DRK => _nonPreservingDRK,");
		sb.AppendLine("\t\tJob.GNB => _nonPreservingGNB,");
		sb.AppendLine("\t\tJob.CNJ or Job.WHM => _nonPreservingWHM,");
		sb.AppendLine("\t\tJob.SCH => _nonPreservingSCH,");
		sb.AppendLine("\t\tJob.AST => _nonPreservingAST,");
		sb.AppendLine("\t\tJob.SGE => _nonPreservingSGE,");
		sb.AppendLine("\t\tJob.PGL or Job.MNK => _nonPreservingMNK,");
		sb.AppendLine("\t\tJob.LNC or Job.DRG => _nonPreservingDRG,");
		sb.AppendLine("\t\tJob.ROG or Job.NIN => _nonPreservingNIN,");
		sb.AppendLine("\t\tJob.SAM => _nonPreservingSAM,");
		sb.AppendLine("\t\tJob.RPR => _nonPreservingRPR,");
		sb.AppendLine("\t\tJob.VPR => _nonPreservingVPR,");
		sb.AppendLine("\t\tJob.ARC or Job.BRD => _nonPreservingBRD,");
		sb.AppendLine("\t\tJob.MCH => _nonPreservingMCH,");
		sb.AppendLine("\t\tJob.DNC => _nonPreservingDNC,");
		sb.AppendLine("\t\tJob.THM or Job.BLM => _nonPreservingBLM,");
		sb.AppendLine("\t\tJob.ACN or Job.SMN => _nonPreservingSMN,");
		sb.AppendLine("\t\tJob.RDM => _nonPreservingRDM,");
		sb.AppendLine("\t\tJob.PCT => _nonPreservingPCT,");
		sb.AppendLine("\t\t_ => [],");
		sb.AppendLine("\t};");
		sb.AppendLine();

		// NormalizeComboStep
		sb.AppendLine("\tpublic static ActionID NormalizeComboStep(ActionID actionId) => actionId switch");
		sb.AppendLine("\t{");
		foreach (var (fromId, toId) in comboStepNormalization)
		{
			if (actionNames.TryGetValue(fromId, out var fromName) && actionNames.TryGetValue(toId, out var toName))
			{
				sb.AppendLine($"\t\tActionID.{fromName} => ActionID.{toName},");
			}
		}
		sb.AppendLine("\t\t_ => actionId,");
		sb.AppendLine("\t};");
		sb.AppendLine();

		// GetActionLevel
		sb.AppendLine("\tpublic static byte GetActionLevel(ActionID actionId) => actionId switch");
		sb.AppendLine("\t{");
		foreach (var (id, level) in actionLevels)
		{
			sb.AppendLine($"\t\tActionID.{actionNames[id]} => {level},");
		}
		sb.AppendLine("\t\t_ => 0,");
		sb.AppendLine("\t};");
		sb.AppendLine();

		// GetInheritedUnlockLink (own or inherited via ReplaceAction / Trait)
		sb.AppendLine("\tpublic static uint GetInheritedUnlockLink(ActionID actionId) => actionId switch");
		sb.AppendLine("\t{");
		foreach (var (id, unlockLink) in actionUnlockLinks)
		{
			if (actionNames.TryGetValue(id, out var name))
			{
				sb.AppendLine($"\t\tActionID.{name} => {unlockLink},");
			}
		}
		sb.AppendLine("\t\t_ => 0,");
		sb.AppendLine("\t};");
		sb.AppendLine();

		// GetTraitLevel
		sb.AppendLine("\tpublic static byte GetTraitLevel(TraitID traitId) => traitId switch");
		sb.AppendLine("\t{");
		foreach (var (id, level) in traitLevels)
		{
			sb.AppendLine($"\t\tTraitID.{traitNames[id]} => {level},");
		}
		sb.AppendLine("\t\t_ => 0,");
		sb.AppendLine("\t};");
		sb.AppendLine();

		// GetTraitQuestId
		sb.AppendLine("\tpublic static uint GetTraitQuestId(TraitID traitId) => traitId switch");
		sb.AppendLine("\t{");
		foreach (var (id, questId) in traitQuests)
		{
			sb.AppendLine($"\t\tTraitID.{traitNames[id]} => {questId},");
		}
		sb.AppendLine("\t\t_ => 0,");
		sb.AppendLine("\t};");

		sb.AppendLine("}");
		return sb.ToString();
	}

	private static Dictionary<uint, string> BuildActionNameMap(Lumina.GameData gameData)
	{
		var map = new Dictionary<uint, string>();
		new ActionNameCollector(gameData).Collect(map);
		return map;
	}

	private static Dictionary<uint, string> BuildTraitNameMap(Lumina.Excel.ExcelSheet<Trait> traits, Lumina.Excel.ExcelSheet<ClassJob> classJobs)
	{
		var nonCombatJobs = new HashSet<uint>();
		foreach (var c in classJobs)
		{
			if (c.ClassJobCategory.RowId is 32 or 33)
			{
				nonCombatJobs.Add(c.RowId);
			}
		}

		var addedNames = new HashSet<string>();
		var map = new Dictionary<uint, string>();

		foreach (var item in traits)
		{
			if (item.ClassJob.RowId == 0 || !item.ClassJob.IsValid || nonCombatJobs.Contains(item.ClassJob.RowId))
			{
				continue;
			}

			if (item.Icon == 0)
			{
				continue;
			}

			var rawName = item.Name.ToString();
			if (string.IsNullOrEmpty(rawName))
			{
				continue;
			}

			var ascii = true;
			foreach (var ch in rawName)
			{
				if (!char.IsAscii(ch))
				{
					ascii = false;
					break;
				}
			}
			if (!ascii)
			{
				continue;
			}

			var name = rawName.ToPascalCase();
			if (!addedNames.Add(name))
			{
				name += "_" + item.RowId.ToString();
			}

			map[item.RowId] = name;
		}

		return map;
	}

	private static HashSet<uint> BuildReplacedByPlayerSet(
		Lumina.Excel.SubrowExcelSheet<ReplaceAction> replaceActions,
		Lumina.Excel.ExcelSheet<ActionIndirection> indirections)
	{
		var replaced = new HashSet<uint>();

		foreach (var row in replaceActions)
		{
			foreach (var sub in row)
			{
				if (sub.Action.RowId == 0 || !sub.Action.IsValid || !sub.Action.Value.IsPlayerAction)
				{
					continue;
				}

				foreach (var rep in sub.ReplaceActions)
				{
					if (rep.RowId > 0)
					{
						replaced.Add(rep.RowId);
					}
				}
			}
		}

		foreach (var indir in indirections)
		{
			if (indir.Name.RowId == 0 || !indir.Name.IsValid || indir.ClassJob.RowId == 0)
			{
				continue;
			}

			if (indir.PreviousComboAction.RowId == 0
				|| (indir.PreviousComboAction.IsValid && (indir.PreviousComboAction.Value.IsPlayerAction || replaced.Contains(indir.PreviousComboAction.RowId))))
			{
				replaced.Add(indir.Name.RowId);
			}
		}

		return replaced;
	}

	private static Dictionary<string, List<string>> BuildNonPreservingByJob(
		Lumina.Excel.ExcelSheet<Action> actions,
		HashSet<uint> replacedByPlayer,
		Dictionary<uint, string> actionNames)
	{
		var byJob = new Dictionary<string, List<string>>(CombatJobAbbreviations.Length);
		foreach (var abbr in CombatJobAbbreviations)
		{
			byJob[abbr] = [];
		}

		foreach (var item in actions)
		{
			if (item.IsPvP || item.PreservesCombo || item.ClassJobLevel == 0 || item.Icon is 0 or 405 or 784)
			{
				continue;
			}

			// Restrict to actual player-usable or hotbar-transformed combat GCDs (excludes pet actions and oGCD abilities).
			if (item.ActionCategory.RowId is not (2 or 3))
			{
				continue;
			}

			if (item.CooldownGroup != 58 && item.AdditionalCooldownGroup != 58)
			{
				continue;
			}

			if (item.ClassJob.RowId == uint.MaxValue)
			{
				continue;
			}

			if (!item.IsPlayerAction && !replacedByPlayer.Contains(item.RowId))
			{
				continue;
			}

			if (!item.ClassJobCategory.IsValid || !actionNames.TryGetValue(item.RowId, out var enumName))
			{
				continue;
			}

			var category = item.ClassJobCategory.Value;
			if (!category.IsSingleJobForCombat())
			{
				continue;
			}

			foreach (var abbr in CombatJobAbbreviations)
			{
				if (IsJobInCategory(category, abbr))
				{
					byJob[abbr].Add(enumName);
				}
			}
		}

		return byJob;
	}

	private static List<(uint FromId, uint ToId)> BuildComboStepNormalization(
		Lumina.Excel.ExcelSheet<Action> actions,
		Lumina.Excel.SubrowExcelSheet<ReplaceAction> replaceActions,
		Lumina.Excel.ExcelSheet<ActionIndirection> indirections,
		Dictionary<uint, string> actionNames)
	{
		var comboPrereqs = new HashSet<uint>();
		foreach (var a in actions)
		{
			if (!a.IsPvP && a.ClassJobLevel > 0 && a.ActionCombo.RowId > 0)
			{
				comboPrereqs.Add(a.ActionCombo.RowId);
			}
		}

		var map = new Dictionary<uint, uint>();
		var replaceByTarget = new Dictionary<uint, HashSet<uint>>();

		foreach (var row in replaceActions)
		{
			foreach (var sub in row)
			{
				if (sub.Action.RowId == 0 || !sub.Action.IsValid)
				{
					continue;
				}

				var baseAction = sub.Action.Value;
				if (baseAction.IsPvP || baseAction.ClassJobLevel == 0)
				{
					continue;
				}

				foreach (var repRef in sub.ReplaceActions)
				{
					if (repRef.RowId == 0 || !repRef.IsValid)
					{
						continue;
					}

					var rep = repRef.Value;
					if (rep.IsPvP || rep.ClassJobLevel == 0 || rep.RowId == baseAction.RowId)
					{
						continue;
					}

					// Same-step upgrade or stance variant of a combo prerequisite (ClassJobCategory may narrow e.g. LNC DRG -> DRG).
					if (comboPrereqs.Contains(baseAction.RowId)
						&& !rep.PreservesCombo
						&& rep.ActionCombo.RowId == baseAction.ActionCombo.RowId)
					{
						map[rep.RowId] = baseAction.RowId;
					}

					if (!replaceByTarget.TryGetValue(rep.RowId, out var sources))
					{
						sources = [];
						replaceByTarget[rep.RowId] = sources;
					}

					sources.Add(baseAction.RowId);
				}
			}
		}

		// Convergent branches where multiple base actions replace into the same finisher (e.g. Wheeling Thrust & Fang and Claw -> Drakesbane).
		foreach (var (targetId, sources) in replaceByTarget)
		{
			var target = actions.GetRow(targetId);
			var canonicalPrereq = target.ActionCombo.RowId;
			if (canonicalPrereq > 0 && sources.Contains(canonicalPrereq))
			{
				foreach (var srcId in sources)
				{
					if (srcId != canonicalPrereq && !actions.GetRow(srcId).PreservesCombo)
					{
						map[srcId] = canonicalPrereq;
					}
				}
			}
		}

		// ActionIndirection sibling combo chains (e.g. Viper Reaving Fangs -> Steel Fangs, Reaving Maw -> Steel Maw, Swiftskin's Bite -> Hunter's Bite).
		var byPrev = new Dictionary<uint, List<Action>>();
		foreach (var indir in indirections)
		{
			if (indir.Name.RowId == 0 || !indir.Name.IsValid || indir.PreviousComboAction.RowId == 0 || !indir.PreviousComboAction.IsValid)
			{
				continue;
			}

			var x = indir.Name.Value;
			var b = indir.PreviousComboAction.Value;
			if (x.IsPvP || b.IsPvP || x.ClassJobLevel == 0 || x.ActionCombo.RowId == 0 || !x.ActionCombo.IsValid)
			{
				continue;
			}

			if (!byPrev.TryGetValue(b.RowId, out var list))
			{
				list = [];
				byPrev[b.RowId] = list;
			}
			list.Add(x);

			var a = x.ActionCombo.Value;
			if (a.RowId != b.RowId
				&& a.ActionCombo.RowId == b.ActionCombo.RowId
				&& a.IsPlayerAction == b.IsPlayerAction
				&& !b.PreservesCombo
				&& comboPrereqs.Contains(a.RowId))
			{
				map[b.RowId] = a.RowId;
			}
		}

		foreach (var (_, list) in byPrev)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var x1 = list[i];
				if (x1.ActionCombo.RowId == 0 || !x1.ActionCombo.IsValid)
				{
					continue;
				}

				for (var j = 0; j < list.Count; j++)
				{
					var x2 = list[j];
					if (x1.RowId != x2.RowId
						&& x1.ActionCombo.RowId != x2.RowId
						&& !x2.PreservesCombo
						&& x2.ActionCombo.RowId == x1.ActionCombo.Value.ActionCombo.RowId)
					{
						map[x2.RowId] = x1.ActionCombo.RowId;
					}
				}
			}
		}

		// Server-side combo mirror: Verholy (7526) mirrors Verflare (7525) into Scorch (16530), whose ActionCombo is Verflare.
		if (actionNames.ContainsKey(7526) && actionNames.ContainsKey(7525) && !map.ContainsKey(7526))
		{
			map[7526] = 7525;
		}

		// Collapse multi-hop mappings to their canonical root and sort deterministically.
		var result = new List<(uint FromId, uint ToId)>(map.Count);
		var keys = new List<uint>(map.Keys);
		keys.Sort();
		foreach (var fromId in keys)
		{
			var target = map[fromId];
			var visited = new HashSet<uint> { fromId };
			while (map.TryGetValue(target, out var next) && visited.Add(target))
			{
				target = next;
			}

			if (target != fromId && actionNames.ContainsKey(fromId) && actionNames.ContainsKey(target))
			{
				result.Add((fromId, target));
			}
		}

		return result;
	}

	private static List<(uint ActionId, uint UnlockLink)> BuildActionUnlockLinks(
		Lumina.Excel.ExcelSheet<Action> actions,
		Lumina.Excel.ExcelSheet<Trait> traits,
		Lumina.Excel.SubrowExcelSheet<ReplaceAction> replaceActions)
	{
		var unlocks = new Dictionary<uint, (uint QuestId, byte SourceLevel)>();

		foreach (var a in actions)
		{
			if (!a.IsPvP && a.UnlockLink.RowId > 0)
			{
				unlocks[a.RowId] = (a.UnlockLink.RowId, a.ClassJobLevel);
			}
		}

		// Propagate quest unlocks along ReplaceAction chains (upgraded actions and procs whose base action or trait has a quest unlock).
		var changed = true;
		while (changed)
		{
			changed = false;
			foreach (var row in replaceActions)
			{
				foreach (var sub in row)
				{
					if (sub.Action.RowId == 0 || !sub.Action.IsValid)
					{
						continue;
					}

					var baseAction = sub.Action.Value;
					if (baseAction.IsPvP || baseAction.ClassJobLevel == 0)
					{
						continue;
					}

					for (var i = 0; i < sub.ReplaceActions.Count; i++)
					{
						var repRef = sub.ReplaceActions[i];
						if (repRef.RowId == 0 || !repRef.IsValid)
						{
							continue;
						}

						var rep = repRef.Value;
						if (rep.IsPvP || rep.ClassJobLevel == 0)
						{
							continue;
						}

						var (type, paramId) = i switch
						{
							0 => (sub.Type1, sub.Param1.RowId),
							1 => (sub.Type2, sub.Param2.RowId),
							2 => (sub.Type3, sub.Param3.RowId),
							_ => (sub.Type4, sub.Param4.RowId),
						};

						uint questId = 0;
						byte questLevel = baseAction.ClassJobLevel;
						if (type == 3 && paramId > 0 && traits.TryGetRow(paramId, out var trait) && trait.Quest.RowId > 0)
						{
							questId = trait.Quest.RowId;
							questLevel = trait.Level;
						}
						else if (unlocks.TryGetValue(baseAction.RowId, out var inherited))
						{
							questId = inherited.QuestId;
							questLevel = inherited.SourceLevel;
						}

						if (questId == 0 || rep.UnlockLink.RowId > 0)
						{
							continue;
						}

						if (!unlocks.TryGetValue(rep.RowId, out var existing) || questLevel > existing.SourceLevel)
						{
							unlocks[rep.RowId] = (questId, questLevel);
							changed = true;
						}
					}
				}
			}
		}

		var result = new List<(uint ActionId, uint UnlockLink)>(unlocks.Count);
		var keys = new List<uint>(unlocks.Keys);
		keys.Sort();
		foreach (var id in keys)
		{
			result.Add((id, unlocks[id].QuestId));
		}

		return result;
	}

	private static List<(uint ActionId, byte Level)> BuildActionLevels(
		Lumina.Excel.ExcelSheet<Action> actions,
		Dictionary<uint, string> actionNames)
	{
		var result = new List<(uint ActionId, byte Level)>();
		foreach (var a in actions)
		{
			if (a.ClassJobLevel > 0 && actionNames.ContainsKey(a.RowId))
			{
				result.Add((a.RowId, a.ClassJobLevel));
			}
		}

		return result;
	}

	private static List<(uint TraitId, byte Level)> BuildTraitLevels(
		Lumina.Excel.ExcelSheet<Trait> traits,
		Dictionary<uint, string> traitNames)
	{
		var result = new List<(uint TraitId, byte Level)>();
		foreach (var t in traits)
		{
			if (t.Level > 0 && traitNames.ContainsKey(t.RowId))
			{
				result.Add((t.RowId, t.Level));
			}
		}

		return result;
	}

	private static List<(uint TraitId, uint QuestId)> BuildTraitQuests(
		Lumina.Excel.ExcelSheet<Trait> traits,
		Dictionary<uint, string> traitNames)
	{
		var result = new List<(uint TraitId, uint QuestId)>();
		foreach (var t in traits)
		{
			if (t.Quest.RowId > 0 && traitNames.ContainsKey(t.RowId))
			{
				result.Add((t.RowId, t.Quest.RowId));
			}
		}

		return result;
	}

	private static bool IsJobInCategory(ClassJobCategory category, string abbr) => abbr switch
	{
		"PLD" => category.PLD,
		"WAR" => category.WAR,
		"DRK" => category.DRK,
		"GNB" => category.GNB,
		"WHM" => category.WHM,
		"SCH" => category.SCH,
		"AST" => category.AST,
		"SGE" => category.SGE,
		"MNK" => category.MNK,
		"DRG" => category.DRG,
		"NIN" => category.NIN,
		"SAM" => category.SAM,
		"RPR" => category.RPR,
		"VPR" => category.VPR,
		"BRD" => category.BRD,
		"MCH" => category.MCH,
		"DNC" => category.DNC,
		"BLM" => category.BLM,
		"SMN" => category.SMN,
		"RDM" => category.RDM,
		"PCT" => category.PCT,
		_ => false,
	};

	private sealed class ActionNameCollector(Lumina.GameData gameData) : ActionGetterBase(gameData)
	{
		private Dictionary<uint, string>? _target;

		public void Collect(Dictionary<uint, string> map)
		{
			_target = map;
			GetCode();
		}

		protected override string ToCode(Action item)
		{
			var name = GetName(item);
			_target![item.RowId] = name;
			return string.Empty;
		}
	}
}
