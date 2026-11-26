using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace NoSillyReborn.Engine.State;

internal static class DataCenter
{
	public static bool PlayerAvailable()
	{
		return Player.Available && Player.Object != null;
	}

	public static bool CanAct()
	{
		return PlayerAvailable()
			&& Player.Object is { IsDead: false }
			&& !Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
			&& !Svc.Condition[ConditionFlag.WatchingCutscene]
			&& !Svc.Condition[ConditionFlag.WatchingCutscene78]
			&& !Svc.Condition[ConditionFlag.Crafting]
			&& !Svc.Condition[ConditionFlag.Gathering]
			&& !Svc.Condition[ConditionFlag.Mounted]
			&& !Svc.Condition[ConditionFlag.Unconscious];
	}

	public static Job Job => PlayerAvailable() ? Player.Job : Job.ADV;

	public static RotationType PlayerRotationType => RotationType.Reborn;

	public static bool InCombat => Svc.Condition[ConditionFlag.InCombat];

	public static bool IsPvP => Svc.ClientState.IsPvP;

	public static bool IsRsrAutorotationActive => RSR_IPCSubscriber.IsAutorotationActive();

	public static int PlayerSyncedLevel()
	{
		if (Player.IsLevelSynced)
		{
			return Player.SyncedLevel;
		}

		return Math.Min(PlayerCurrentLevel, PlayerMaxLevel);
	}

	public static unsafe int PlayerCurrentLevel => PlayerState.Instance() is var state && state != null ? state->CurrentLevel : 0;

	public static int PlayerMaxLevel => Player.MaxLevel;

	public static unsafe float ComboTime => ActionManager.Instance() is var am && am != null ? am->Combo.Timer : 0f;

	public static unsafe ActionID ComboAction => ActionManager.Instance() is var am && am != null && am->Combo.Timer > 0f
		? (ActionID)am->Combo.Action
		: ActionID.None;

	public static bool HasActiveCombo => ComboAction != ActionID.None;
}
