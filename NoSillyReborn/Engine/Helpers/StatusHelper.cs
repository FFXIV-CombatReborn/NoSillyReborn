using ECommons.GameFunctions;
using ECommons.GameHelpers;
using GameStatus = FFXIVClientStructs.FFXIV.Client.Game.Status;

namespace NoSillyReborn.Engine.Helpers;

public static partial class StatusHelper
{
	internal static unsafe ReadOnlySpan<GameStatus> GetStatuses(this IBattleChara? battleChara)
	{
		if (battleChara == null || battleChara.Address == nint.Zero || !battleChara.IsValid())
		{
			return default;
		}

		var chara = battleChara.Struct();
		if (chara == null)
		{
			return default;
		}

		var statuses = chara->StatusManager.Status;
		int count = chara->StatusManager.NumValidStatuses;
		if (count <= 0)
		{
			return default;
		}

		return count < statuses.Length ? statuses[..count] : statuses;
	}

	public static bool PlayerHasStatus(bool isFromSelf, params ReadOnlySpan<StatusID> statusIDs)
	{
		var player = Player.Object;
		return player != null && AnyStatusMatches(player, isFromSelf, statusIDs);
	}

	public static bool HasStatus(this IBattleChara battleChara, bool isFromSelf, params ReadOnlySpan<StatusID> statusIDs)
	{
		try
		{
			if (battleChara == null || !DataCenter.PlayerAvailable() || !battleChara.IsValid())
			{
				return false;
			}

			return AnyStatusMatches(battleChara, isFromSelf, statusIDs);
		}
		catch
		{
			return false;
		}
	}

	public static float PlayerStatusTime(bool isFromSelf, params ReadOnlySpan<StatusID> statusIDs)
	{
		var player = Player.Object;
		if (player == null)
		{
			return 0f;
		}

		var min = MinStatusRemainingTime(player, isFromSelf, statusIDs, out var found);
		return found ? min : 0f;
	}

	public static float StatusTime(this IBattleChara battleChara, bool isFromSelf, params ReadOnlySpan<StatusID> statusIDs)
	{
		var min = MinStatusRemainingTime(battleChara, isFromSelf, statusIDs, out var found);
		return found ? min : 0f;
	}

	public static float PlayerLongestStatusTime(bool isFromSelf, params ReadOnlySpan<StatusID> statusIDs)
	{
		var player = Player.Object;
		if (player == null)
		{
			return 0f;
		}

		var max = MaxStatusRemainingTime(player, isFromSelf, statusIDs, out var found);
		return found ? max : 0f;
	}

	public static byte PlayerStatusStack(bool isFromSelf, params ReadOnlySpan<StatusID> statusIDs)
	{
		var player = Player.Object;
		if (player == null)
		{
			return 0;
		}

		var min = MinStatusStack(player, isFromSelf, statusIDs, out var found);
		return found ? min : (byte)0;
	}

	public static byte StatusStack(this IBattleChara battleChara, bool isFromSelf, params ReadOnlySpan<StatusID> statusIDs)
	{
		var min = MinStatusStack(battleChara, isFromSelf, statusIDs, out var found);
		return found ? min : (byte)0;
	}

	private static bool ContainsId(uint id, ReadOnlySpan<StatusID> ids)
	{
		for (var i = 0; i < ids.Length; i++)
		{
			if ((uint)ids[i] == id)
			{
				return true;
			}
		}
		return false;
	}

	private static bool Matches(in GameStatus status, bool isFromSelf, ReadOnlySpan<StatusID> statusIDs, ulong playerId)
	{
		return status.StatusId != 0 && ContainsId(status.StatusId, statusIDs) && (!isFromSelf || IsFromPlayer(status, playerId));
	}

	private static bool AnyStatusMatches(IBattleChara? battleChara, bool isFromSelf, ReadOnlySpan<StatusID> statusIDs)
	{
		var statuses = battleChara.GetStatuses();
		if (statuses.IsEmpty)
		{
			return false;
		}

		var playerId = isFromSelf ? Player.Object?.GameObjectId ?? 0 : 0;
		foreach (ref readonly var status in statuses)
		{
			if (Matches(status, isFromSelf, statusIDs, playerId))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsFromPlayer(in GameStatus status, ulong playerId)
	{
		if (playerId == 0)
		{
			return false;
		}

		var sourceId = status.SourceObject.ObjectId;
		if (sourceId == playerId)
		{
			return true;
		}

		if (sourceId == 0 || sourceId == TargetHelper.InvalidTargetId)
		{
			return false;
		}

		return Svc.Objects.SearchById(sourceId)?.OwnerId == playerId;
	}

	private static float RemainingTime(in GameStatus status)
	{
		return status.RemainingTime == 0f ? float.MaxValue : MathF.Max(0f, status.RemainingTime);
	}

	internal static float MinStatusRemainingTime(IBattleChara? battleChara, bool isFromSelf, ReadOnlySpan<StatusID> statusIDs, out bool found)
	{
		found = false;
		var min = float.MaxValue;
		var playerId = isFromSelf ? Player.Object?.GameObjectId ?? 0 : 0;
		foreach (ref readonly var status in battleChara.GetStatuses())
		{
			if (Matches(status, isFromSelf, statusIDs, playerId))
			{
				min = MathF.Min(min, RemainingTime(status));
				found = true;
			}
		}
		return min;
	}

	internal static float MaxStatusRemainingTime(IBattleChara? battleChara, bool isFromSelf, ReadOnlySpan<StatusID> statusIDs, out bool found)
	{
		found = false;
		var max = 0f;
		var playerId = isFromSelf ? Player.Object?.GameObjectId ?? 0 : 0;
		foreach (ref readonly var status in battleChara.GetStatuses())
		{
			if (Matches(status, isFromSelf, statusIDs, playerId))
			{
				max = MathF.Max(max, RemainingTime(status));
				found = true;
			}
		}
		return max;
	}

	private static byte MinStatusStack(IBattleChara? battleChara, bool isFromSelf, ReadOnlySpan<StatusID> statusIDs, out bool found)
	{
		found = false;
		var min = byte.MaxValue;
		var playerId = isFromSelf ? Player.Object?.GameObjectId ?? 0 : 0;
		foreach (ref readonly var status in battleChara.GetStatuses())
		{
			if (Matches(status, isFromSelf, statusIDs, playerId))
			{
				var stack = status.Param == 0 ? byte.MaxValue : (byte)Math.Min(status.Param, byte.MaxValue);
				if (stack < min)
				{
					min = stack;
				}
				found = true;
			}
		}
		return min;
	}
}
