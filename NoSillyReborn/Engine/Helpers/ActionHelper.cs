using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Action = Lumina.Excel.Sheets.Action;
using Trait = Lumina.Excel.Sheets.Trait;

namespace NoSillyReborn.Engine.Helpers;

internal static class ActionHelper
{
	internal const byte GCDCooldownGroup = 58;

	private static readonly Dictionary<uint, (string Name, uint Icon)> _display = [];
	private static ExcelSheet<Action>? _sheet;
	private static ExcelSheet<Trait>? _traitSheet;

	private static ExcelSheet<Action> Sheet => _sheet ??= Service.GetSheet<Action>();

	private static ExcelSheet<Trait> TraitSheet => _traitSheet ??= Service.GetSheet<Trait>();

	public static bool TryGetAction(uint actionId, out Action action)
	{
		return Sheet.TryGetRow(actionId, out action);
	}

	public static bool TryGetTrait(uint traitId, out Trait trait)
	{
		return TraitSheet.TryGetRow(traitId, out trait);
	}

	public static ActionCate GetActionCate(this Action action)
	{
		return (ActionCate)(action.ActionCategory.IsValid ? action.ActionCategory.Value.RowId : 0);
	}

	public static bool IsRealGCD(this Action action)
	{
		return action.CooldownGroup == GCDCooldownGroup || action.AdditionalCooldownGroup == GCDCooldownGroup;
	}

	public static bool IsRealGCD(uint actionId)
	{
		return TryGetAction(actionId, out var action) && action.IsRealGCD();
	}

	public static byte GetActionLevel(this ActionID actionId)
	{
		return TryGetAction((uint)actionId, out var action)
			? action.ClassJobLevel
			: GameMetadata.GetActionLevel(actionId);
	}

	public static bool EnoughLevel(this ActionID actionId)
	{
		return DataCenter.PlayerSyncedLevel() >= actionId.GetActionLevel();
	}

	public static unsafe bool IsQuestUnlocked(this ActionID actionId)
	{
		var liveUnlock = TryGetAction((uint)actionId, out var action) ? action.UnlockLink.RowId : 0u;
		var inheritedUnlock = GameMetadata.GetInheritedUnlockLink(actionId);
		if (liveUnlock == 0 && inheritedUnlock == 0)
		{
			return true;
		}

		var uiState = UIState.Instance();
		if (uiState == null)
		{
			return true;
		}

		if (liveUnlock != 0 && !uiState->IsUnlockLinkUnlockedOrQuestCompleted(liveUnlock))
		{
			return false;
		}

		if (inheritedUnlock != 0 && inheritedUnlock != liveUnlock && !uiState->IsUnlockLinkUnlockedOrQuestCompleted(inheritedUnlock))
		{
			return false;
		}

		return true;
	}

	public static bool IsUnlocked(this ActionID actionId)
	{
		return actionId.EnoughLevel() && actionId.IsQuestUnlocked();
	}

	public static byte GetTraitLevel(this TraitID traitId)
	{
		return TryGetTrait((uint)traitId, out var trait)
			? trait.Level
			: GameMetadata.GetTraitLevel(traitId);
	}

	public static bool EnoughLevel(this TraitID traitId)
	{
		return DataCenter.PlayerSyncedLevel() >= traitId.GetTraitLevel();
	}

	public static unsafe bool IsQuestUnlocked(this TraitID traitId)
	{
		var liveQuest = TryGetTrait((uint)traitId, out var trait) ? trait.Quest.RowId : 0u;
		var generatedQuest = GameMetadata.GetTraitQuestId(traitId);
		if (liveQuest == 0 && generatedQuest == 0)
		{
			return true;
		}

		var uiState = UIState.Instance();
		if (uiState == null)
		{
			return true;
		}

		if (liveQuest != 0 && !uiState->IsUnlockLinkUnlockedOrQuestCompleted(liveQuest))
		{
			return false;
		}

		if (generatedQuest != 0 && generatedQuest != liveQuest && !uiState->IsUnlockLinkUnlockedOrQuestCompleted(generatedQuest))
		{
			return false;
		}

		return true;
	}

	public static bool IsUnlocked(this TraitID traitId)
	{
		return traitId.EnoughLevel() && traitId.IsQuestUnlocked();
	}

	public static bool PreservesCombo(this Action action)
	{
		return action.PreservesCombo;
	}

	public static bool PreservesCombo(uint actionId)
	{
		return TryGetAction(actionId, out var action) && action.PreservesCombo;
	}

	public static bool PreservesCombo(this ActionID actionId)
	{
		return PreservesCombo((uint)actionId);
	}

	public static ActionID GetComboPrerequisite(this ActionID actionId)
	{
		return TryGetAction((uint)actionId, out var action) ? (ActionID)action.ActionCombo.RowId : ActionID.None;
	}

	public static bool ContinuesCombo(this ActionID actionId, ActionID activeCombo)
	{
		if (activeCombo == ActionID.None)
		{
			return false;
		}

		var prerequisite = actionId.GetComboPrerequisite();
		return prerequisite != ActionID.None
			&& GameMetadata.NormalizeComboStep(prerequisite) == GameMetadata.NormalizeComboStep(activeCombo);
	}

	public static bool HasUnlockedComboContinuation(Job job, ActionID activeCombo)
	{
		if (activeCombo == ActionID.None)
		{
			return false;
		}

		var normalized = GameMetadata.NormalizeComboStep(activeCombo);
		foreach (var candidate in GameMetadata.NonPreservingComboActions(job))
		{
			var prerequisite = candidate.GetComboPrerequisite();
			if (prerequisite != ActionID.None
				&& GameMetadata.NormalizeComboStep(prerequisite) == normalized
				&& candidate.IsUnlocked())
			{
				return true;
			}
		}

		return false;
	}

	public static bool WouldBreakCombo(this ActionID actionId)
	{
		var activeCombo = DataCenter.ComboAction;
		return activeCombo != ActionID.None
			&& !actionId.PreservesCombo()
			&& !actionId.ContinuesCombo(activeCombo)
			&& HasUnlockedComboContinuation(DataCenter.Job, activeCombo);
	}

	public static string GetName(this ActionID actionId)
	{
		return Display(actionId).Name;
	}

	public static uint GetIcon(this ActionID actionId)
	{
		return Display(actionId).Icon;
	}

	private static (string Name, uint Icon) Display(ActionID actionId)
	{
		var id = (uint)actionId;
		if (_display.TryGetValue(id, out var display))
		{
			return display;
		}

		display = TryGetAction(id, out var action)
			? (action.Name.ExtractText(), action.Icon)
			: (actionId.ToString(), 0u);
		_display[id] = display;
		return display;
	}
}
