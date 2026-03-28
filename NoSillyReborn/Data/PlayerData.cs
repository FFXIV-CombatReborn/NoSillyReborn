using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;

#nullable enable
namespace NoSillyReborn.Data;

internal static class PlayerData
{
    public static int PlayerSyncedLevel()
    {
        if (Player.IsLevelSynced)
            return Player.SyncedLevel;
        return PlayerCurrentLevel < PlayerMaxLevel ? PlayerCurrentLevel : PlayerMaxLevel;
    }

    public static unsafe int PlayerCurrentLevel => (int)PlayerState.Instance()->CurrentLevel;

    public static int PlayerMaxLevel => (int)Player.MaxLevel;

    public static bool PlayerAvailable() => Player.Available && Player.Object != null;

    public static unsafe ActionID LastComboAction
    {
        get => (ActionID)ActionManager.Instance()->Combo.Action;
    }

    #region GCD
    /// <summary>
    /// Returns the current animation lock remaining time (seconds).
    /// </summary>
    public static float AnimationLock => Player.AnimationLock;

    /// <summary>
    /// Time until the next ability relative to the next GCD window.
    /// Non-negative (clamped to 0).
    /// </summary>
    public static float NextAbilityToNextGCD => Math.Max(0f, DefaultGCDRemain - AnimationLock);

    /// <summary>
    /// Returns the total duration of the default GCD (seconds). Clamped to non-negative.
    /// </summary>
    public static float DefaultGCDTotal => Math.Max(0f, ActionManagerHelper.GetDefaultRecastTime());

    /// <summary>
    /// Returns the remaining time for the default GCD by subtracting the elapsed time from the total recast time.
    /// Clamped to non-negative.
    /// </summary>
    public static float DefaultGCDRemain => Math.Max(0f, DefaultGCDTotal - DefaultGCDElapsed);

    /// <summary>
    /// Returns the elapsed time since the start of the default GCD. Clamped to non-negative.
    /// </summary>
    public static float DefaultGCDElapsed => Math.Max(0f, ActionManagerHelper.GetDefaultRecastTimeElapsed());

    /// <summary>
    /// Calculates the action ahead time based on the default GCD total and configured multiplier.
    /// Result is clamped to non-negative.
    /// </summary>
    public static float CalculatedActionAhead => Math.Max(0f, DefaultGCDTotal * 0.6f);

    /// <summary>
    /// Calculates the total GCD time for a given number of GCDs and an optional offset.
    /// </summary>
    /// <param name="gcdCount">The number of GCDs.</param>
    /// <param name="offset">The optional offset.</param>
    /// <returns>The total GCD time.</returns>
    public static float GCDTime(uint gcdCount = 0, float offset = 0)
    {
        return (DefaultGCDTotal * gcdCount) + offset;
    }
    #endregion

    internal static Dictionary<ulong, uint> ApplyStatus { get; set; } = [];

    internal static DateTime EffectTime { private get; set; } = DateTime.Now;

    internal static DateTime EffectEndTime { private get; set; } = DateTime.Now;

    internal static bool InEffectTime
    {
        get => DateTime.Now >= EffectTime && DateTime.Now <= EffectEndTime;
    }

    public static uint[] BluSlots { get; internal set; } = new uint[24];

    public static uint[] DutyActions { get; internal set; } = new uint[5];

    private static unsafe void UpdateSlots()
    {
        var actionManager = ActionManager.Instance();
        if (actionManager == null)
        {
            return;
        }

        for (var i = 0; i < BluSlots.Length; i++)
        {
            BluSlots[i] = actionManager->GetActiveBlueMageActionInSlot(i);
        }
        for (ushort i = 0; i < DutyActions.Length; i++)
        {
            DutyActions[i] = DutyActionManager.GetDutyActionId(i);
        }
    }

    #region Territory Info Tracking

    public static Data.TerritoryInfo? Territory { get; set; }
    public static ushort TerritoryID => Svc.ClientState.TerritoryType;

    public static bool IsPvP => Territory?.IsPvP ?? false;

    public static bool IsInDuty => Svc.Condition[ConditionFlag.BoundByDuty] || Svc.Condition[ConditionFlag.BoundByDuty56];

    public static bool IsInAllianceRaid
    {
        get
        {
            ushort[] allianceTerritoryIds =
            [
                151, 174, 372, 508, 556, 627, 734, 776, 826, 882, 917, 966, 1054, 1118, 1178, 1248, 1241
            ];

            for (var i = 0; i < allianceTerritoryIds.Length; i++)
            {
                if (allianceTerritoryIds[i] == TerritoryID)
                    return true;
            }

            return false;
        }
    }

    public static bool IsInUCoB => TerritoryID == 733;
    public static bool IsInUwU => TerritoryID == 777;
    public static bool IsInTEA => TerritoryID == 887;
    public static bool IsInDSR => TerritoryID == 968;
    public static bool IsInTOP => TerritoryID == 1122;
    public static bool IsInFRU => TerritoryID == 1238;
    public static bool IsInCOD => TerritoryID == 1241;

    public static bool IsInM9S => TerritoryID == 1321;
    public static bool IsInM10S => TerritoryID == 1323;
    public static bool IsInM11S => TerritoryID == 1327;
    public static bool IsInM12S => TerritoryID == 1325;


    public static bool IsInTerritory(ushort territoryId)
    {
        return TerritoryID == territoryId;
    }

    #endregion

    #region FATE
    /// <summary>
    /// 
    /// </summary>
    public static unsafe ushort PlayerFateId
    {
        get
        {
            try
            {
                if ((IntPtr)FateManager.Instance() != IntPtr.Zero
                    && (IntPtr)FateManager.Instance()->CurrentFate != IntPtr.Zero
                    && PlayerSyncedLevel() <= FateManager.Instance()->CurrentFate->MaxLevel)
                {
                    return FateManager.Instance()->CurrentFate->FateId;
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex.StackTrace ?? ex.Message);
            }

            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool IsInFate => PlayerFateId != 0;

    #endregion

    #region Variant Dungeon
    /// <summary>
    /// 
    /// </summary>
    public static bool TheMerchantsTaleAdvanced => IsInTerritory(1316);

    /// <summary>
    /// 
    /// </summary>
    public static bool TheMerchantsTale => IsInTerritory(1315);

    /// <summary>
    /// 
    /// </summary>
    public static bool SildihnSubterrane => IsInTerritory(1069);

    /// <summary>
    /// 
    /// </summary>
    public static bool MountRokkon => IsInTerritory(1137);

    /// <summary>
    /// 
    /// </summary>
    public static bool AloaloIsland => IsInTerritory(1176);

    /// <summary>
    /// 
    /// </summary>
    public static bool InVariantDungeon => TheMerchantsTaleAdvanced || TheMerchantsTale || AloaloIsland || MountRokkon || SildihnSubterrane;
    #endregion

    #region Misc Duty Info
    /// <summary>
    /// Determines if the current content is a Monster Hunter duty
    /// </summary>
    public static bool IsInMonsterHunterDuty => RathalosNormal || RathalosEX || ArkveldNormal || ArkveldEX;

    /// <summary>
    /// 
    /// </summary>
    public static bool RathalosNormal => IsInTerritory(761);

    /// <summary>
    /// 
    /// </summary>
    public static bool RathalosEX => IsInTerritory(762);

    /// <summary>
    /// 
    /// </summary>
    public static bool ArkveldNormal => IsInTerritory(1300);

    /// <summary>
    /// 
    /// </summary>
    public static bool ArkveldEX => IsInTerritory(1306);

    /// <summary>
    /// 
    /// </summary>
    public static bool Orbonne => IsInTerritory(826);

    /// <summary>
    /// 
    /// </summary>
    public static bool Emanation => IsInTerritory(719);

    /// <summary>
    /// 
    /// </summary>
    public static bool EmanationEX => IsInTerritory(720);

    public static Job Job => Player.Job;

    public static JobRole Role
    {
        get
        {
            var classJob = Plugin.GetSheet<ClassJob>().GetRow((uint)Job);
            return classJob.RowId != 0 ? classJob.GetJobRole() : JobRole.None;
        }
    }

    public static float JobRange
    {
        get
        {
            float radius = 25;
            if (!Player.Available)
            {
                return radius;
            }

            switch (Role)
            {
                case JobRole.Tank:
                case JobRole.Melee:
                    radius = 3;
                    break;
            }

            return radius;
        }
    }

    public static unsafe bool SylphManagementFinished()
    {
        if (UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(66049))
            return true;

        return false;
    }

    /// <summary>
	/// Returns true if the current class is a base class (pre-jobstone), otherwise false.
	/// </summary>
	public static bool BaseClass()
    {
        // FFXIV base classes: 1-7, 26, 29 (GLA, PGL, MRD, LNC, ARC, CNJ, THM, ACN, ROG)
        if (Svc.Objects.LocalPlayer == null) return false;
        var rowId = Svc.Objects.LocalPlayer.ClassJob.RowId;
        return (rowId >= 1 && rowId <= 7) || rowId == 26 || rowId == 29;
    }
    #endregion
}
