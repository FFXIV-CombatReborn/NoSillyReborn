using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace NoSillyReborn.Engine.Helpers;

internal static class TargetHelper
{
	public const ulong InvalidTargetId = 0xE0000000;

	private static readonly uint[] RaiseActions =
	[
		(uint)ActionID.RaisePvE,
		(uint)ActionID.ResurrectionPvE,
		(uint)ActionID.AscendPvE,
		(uint)ActionID.EgeiroPvE,
		(uint)ActionID.VerraisePvE,
	];

	public static IGameObject? Resolve(ulong targetId)
	{
		if (targetId != 0 && targetId != InvalidTargetId)
		{
			return Svc.Objects.SearchById(targetId);
		}

		return Svc.Targets.SoftTarget ?? Svc.Targets.Target;
	}

	public static unsafe IGameObject? ResolveForAction(ActionID actionId, ulong targetId)
	{
		var target = Resolve(targetId);
		var canTargetSelf = ActionHelper.TryGetAction((uint)actionId, out var action) && action.CanTargetSelf;
		if (target != null && canTargetSelf)
		{
			if (target.Address == nint.Zero || !ActionManager.CanUseActionOnTarget((uint)actionId, (GameObject*)target.Address))
			{
				return Player.Object;
			}
		}

		return target ?? (canTargetSelf ? Player.Object : null);
	}

	public static bool IsBeingRaised(this IGameObject? target)
	{
		if (target is not IBattleChara chara || !chara.IsDead)
		{
			return false;
		}

		if (chara.HasStatus(false, StatusHelper.PendingRaiseStatus))
		{
			return true;
		}

		var playerId = Player.Object?.GameObjectId ?? 0;
		foreach (var obj in Svc.Objects)
		{
			if (obj is IBattleChara caster && caster.GameObjectId != playerId && caster.IsCasting
				&& caster.CastActionType == (byte)ActionType.Action
				&& caster.CastTargetObjectId == chara.GameObjectId && Array.IndexOf(RaiseActions, caster.CastActionId) >= 0)
			{
				return true;
			}
		}

		return false;
	}
}
