using Dalamud.Hooking;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace NoSillyReborn.Intercept;

internal sealed unsafe class HoldCaster : IDisposable
{
	private const float QueueWindowSeconds = 0.5f;
	private const long RepeatIntervalMs = 50;

	private static readonly StatusID[] ChanneledStatuses =
	[
		StatusID.PassageOfArms,
		StatusID.Flamethrower,
		StatusID.Meditate,
		StatusID.CollectiveUnconscious,
		StatusID.Improvisation,
		StatusID.PhantomFlurry,
	];

	private static readonly uint[] OncePerHoldGcds =
	[
		(uint)ActionID.RaisePvE,
		(uint)ActionID.ResurrectionPvE,
		(uint)ActionID.AscendPvE,
		(uint)ActionID.EgeiroPvE,
		(uint)ActionID.VerraisePvE,
		(uint)ActionID.SummonCarbunclePvE,
		(uint)ActionID.SummonEosPvE,
		(uint)ActionID.EukrasiaPvE,
		(uint)ActionID.TenPvE,
		(uint)ActionID.TenPvE_18805,
		(uint)ActionID.ChiPvE,
		(uint)ActionID.ChiPvE_18806,
		(uint)ActionID.JinPvE,
		(uint)ActionID.JinPvE_18807,
		(uint)ActionID.StandardStepPvE,
		(uint)ActionID.TechnicalStepPvE,
		(uint)ActionID.EmboitePvE,
		(uint)ActionID.EntrechatPvE,
		(uint)ActionID.JetePvE,
		(uint)ActionID.PirouettePvE,
		(uint)ActionID.StandardFinishPvE,
		(uint)ActionID.SingleStandardFinishPvE,
		(uint)ActionID.DoubleStandardFinishPvE,
		(uint)ActionID.TechnicalFinishPvE,
		(uint)ActionID.SingleTechnicalFinishPvE,
		(uint)ActionID.DoubleTechnicalFinishPvE,
		(uint)ActionID.TripleTechnicalFinishPvE,
		(uint)ActionID.QuadrupleTechnicalFinishPvE,
		(uint)ActionID.SoulsowPvE,
		(uint)ActionID.FlamethrowerPvE,
		(uint)ActionID.CreatureMotifPvE,
		(uint)ActionID.WeaponMotifPvE,
		(uint)ActionID.LandscapeMotifPvE,
		(uint)ActionID.PomMotifPvE,
		(uint)ActionID.WingMotifPvE,
		(uint)ActionID.ClawMotifPvE,
		(uint)ActionID.MawMotifPvE,
		(uint)ActionID.HammerMotifPvE,
		(uint)ActionID.StarrySkyMotifPvE,
	];

	private static HoldCaster? _instance;

	private readonly Hook<InputData.Delegates.IsInputIdPressed>? _isInputIdPressedHook;
	private readonly Hook<RaptureHotbarModule.Delegates.ExecuteSlot>? _executeSlotHook;

	private InputId _heldInputId;
	private RaptureHotbarModule.HotbarSlot* _heldSlot;
	private uint _heldActionId;
	private int _heldFiredCount;
	private bool _heldNeedsQueueOverride;
	private InputId _completedSingleInputId;
	private long _nextRepeatTick;
	private InputId _pendingInputId;
	private bool _pendingInputRepeat;
	private bool _pendingAllowQueueOverride;
	private bool _faulted;

	public HoldCaster()
	{
		try
		{
			_isInputIdPressedHook = Svc.Hook.HookFromAddress<InputData.Delegates.IsInputIdPressed>(InputData.Addresses.IsInputIdPressed.Value, IsInputIdPressedDetour);
			_executeSlotHook = Svc.Hook.HookFromAddress<RaptureHotbarModule.Delegates.ExecuteSlot>(RaptureHotbarModule.Addresses.ExecuteSlot.Value, ExecuteSlotDetour);
			_isInputIdPressedHook.Enable();
			_executeSlotHook.Enable();
			_instance = this;
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[HoldCaster] Failed to initialize hooks: {ex}");
			Dispose();
		}
	}

	public bool IsActive => _isInputIdPressedHook is { IsEnabled: true, IsDisposed: false }
		&& _executeSlotHook is { IsEnabled: true, IsDisposed: false };

	internal static bool IsRepeating { get; private set; }

	internal static void NotifyActionExecuted(uint executedActionId)
	{
		_instance?.RecordActionExecuted(executedActionId);
	}

	public void Dispose()
	{
		if (ReferenceEquals(_instance, this))
		{
			_instance = null;
		}

		_isInputIdPressedHook?.Disable();
		_isInputIdPressedHook?.Dispose();
		_executeSlotHook?.Disable();
		_executeSlotHook?.Dispose();
		ResetState();
		IsRepeating = false;
	}

	private bool IsInputIdPressedDetour(InputData* inputData, InputId inputId)
	{
		var original = _isInputIdPressedHook!.Original(inputData, inputId);
		if (!IsHotbarSlotInput(inputId))
		{
			return original;
		}

		try
		{
			if (original)
			{
				if (!ShouldRun())
				{
					ResetState();
					return true;
				}

				_heldInputId = inputId;
				_heldSlot = ResolveSlot(inputId);
				_heldActionId = 0;
				_heldFiredCount = 0;
				_heldNeedsQueueOverride = false;
				_completedSingleInputId = default;
				_nextRepeatTick = Environment.TickCount64 + GetHoldDelayMs();
				_pendingInputId = inputId;
				_pendingInputRepeat = false;
				_pendingAllowQueueOverride = false;
				return true;
			}

			_pendingInputId = default;
			_pendingInputRepeat = false;
			_pendingAllowQueueOverride = false;

			if (!ShouldRun())
			{
				ResetState();
				return false;
			}

			if (inputData == null || !inputData->IsInputIdDown(inputId))
			{
				if (_heldInputId == inputId)
				{
					ClearKeyHold();
				}

				if (_completedSingleInputId == inputId)
				{
					_completedSingleInputId = default;
				}

				return false;
			}

			if (inputId == _completedSingleInputId)
			{
				return false;
			}

			if (_heldInputId != inputId)
			{
				if (_heldInputId != default && inputData->IsInputIdDown(_heldInputId))
				{
					if (_heldFiredCount > 0 && IsSingleActivation(_heldSlot))
					{
						_completedSingleInputId = _heldInputId;
						ClearKeyHold();
					}
					else
					{
						return false;
					}
				}

				_heldInputId = inputId;
				_heldSlot = ResolveSlot(inputId);
				_heldFiredCount = 0;
				_heldNeedsQueueOverride = true;
				_nextRepeatTick = Environment.TickCount64 + GetHoldDelayMs();
				return false;
			}

			if (_heldFiredCount > 0 && IsSingleActivation(_heldSlot))
			{
				_completedSingleInputId = inputId;
				ClearKeyHold();
				return false;
			}

			var now = Environment.TickCount64;
			if (now < _nextRepeatTick)
			{
				return false;
			}

			if (IsKeyboardHotbarInput(inputId))
			{
				_heldSlot = ResolveSlot(inputId);
				if (_heldSlot == null)
				{
					return false;
				}
			}

			var allowQueueOverride = _heldNeedsQueueOverride;
			if (!CanRepeat(_heldSlot, _heldFiredCount, allowQueueOverride))
			{
				return false;
			}

			_nextRepeatTick = now + RepeatIntervalMs;
			_heldNeedsQueueOverride = false;
			_pendingInputId = inputId;
			_pendingInputRepeat = true;
			_pendingAllowQueueOverride = allowQueueOverride;
			return true;
		}
		catch (Exception ex)
		{
			if (!_faulted)
			{
				_faulted = true;
				PluginLog.Error($"[HoldCaster] Failed while checking input {inputId}: {ex}");
			}

			ResetState();
			return original;
		}
	}

	private byte ExecuteSlotDetour(RaptureHotbarModule* hotbarModule, RaptureHotbarModule.HotbarSlot* hotbarSlot)
	{
		var pendingId = _pendingInputId;
		var pendingRepeat = _pendingInputRepeat;
		var pendingAllowQueueOverride = _pendingAllowQueueOverride;
		_pendingInputId = default;
		_pendingInputRepeat = false;
		_pendingAllowQueueOverride = false;

		if (pendingId != default)
		{
			if (pendingId == _heldInputId)
			{
				_heldSlot = hotbarSlot;
				_heldActionId = ResolveEffectiveSlotActionId(hotbarSlot);
			}

			if (pendingRepeat)
			{
				if (!CanRepeat(hotbarSlot, _heldFiredCount, pendingAllowQueueOverride))
				{
					return 0;
				}

				IsRepeating = true;
				try
				{
					return _executeSlotHook!.Original(hotbarModule, hotbarSlot);
				}
				finally
				{
					IsRepeating = false;
				}
			}
		}

		return _executeSlotHook!.Original(hotbarModule, hotbarSlot);
	}

	private void RecordActionExecuted(uint executedActionId)
	{
		if (_heldInputId == default)
		{
			return;
		}

		if (IsRepeating || _heldActionId == 0 || executedActionId == _heldActionId)
		{
			_heldFiredCount++;
			return;
		}

		if (_heldSlot != null && TryGetSlotActionId(_heldSlot, out var slotActionId) && executedActionId == slotActionId)
		{
			_heldFiredCount++;
		}
	}

	private void ClearKeyHold()
	{
		_heldInputId = default;
		_heldSlot = null;
		_heldActionId = 0;
		_heldFiredCount = 0;
		_heldNeedsQueueOverride = false;
		_nextRepeatTick = 0;
	}

	private void ResetState()
	{
		ClearKeyHold();
		_completedSingleInputId = default;
		_pendingInputId = default;
		_pendingInputRepeat = false;
		_pendingAllowQueueOverride = false;
	}

	private static bool IsHotbarSlotInput(InputId inputId)
	{
		return inputId is (>= InputId.HOTBAR_1_1 and <= InputId.HOTBAR_CONTENTS_ACT_R)
			or (>= InputId.HOT_PAD_LL and <= InputId.HOT_PAD_RD_R);
	}

	private static bool IsKeyboardHotbarInput(InputId inputId)
	{
		return inputId is >= InputId.HOTBAR_1_1 and <= InputId.HOTBAR_CONTENTS_ACT_R;
	}

	private static long GetHoldDelayMs()
	{
		var seconds = Math.Clamp(Service.Config.HoldToCastDelay, Configs.MinHoldDelay, Configs.MaxHoldDelay);
		return (long)(seconds * 1000f);
	}

	private static bool ShouldRun()
	{
		var config = Service.Config;
		if (!config.Enabled || !config.HoldToCast || !DataCenter.CanAct() || DataCenter.IsPvP || DataCenter.IsRsrAutorotationActive)
		{
			return false;
		}

		if (config.OnlyInCombat && !DataCenter.InCombat)
		{
			return false;
		}

		var hotbarModule = RaptureHotbarModule.Instance();
		if (hotbarModule == null || !hotbarModule->ModuleReady || (hotbarModule->CrossHotbarFlags & CrossHotbarFlags.EditMode) != 0)
		{
			return false;
		}

		var atkStage = AtkStage.Instance();
		return atkStage != null && atkStage->AtkInputManager != null && !atkStage->AtkInputManager->IsTextInputActive;
	}

	private static RaptureHotbarModule.HotbarSlot* ResolveSlot(InputId inputId)
	{
		var hotbarModule = RaptureHotbarModule.Instance();
		if (hotbarModule == null)
		{
			return null;
		}

		if (inputId is >= InputId.HOTBAR_1_1 and <= InputId.HOTBAR_10_B)
		{
			var index = (int)inputId - (int)InputId.HOTBAR_1_1;
			return hotbarModule->GetSlotById((uint)(index / 12), (uint)(index % 12));
		}

		if (inputId is >= InputId.HOTBAR_EX_1 and <= InputId.HOTBAR_EX_B)
		{
			return hotbarModule->PetHotbar.GetHotbarSlot((uint)(inputId - InputId.HOTBAR_EX_1));
		}

		if (inputId is InputId.HOTBAR_CONTENTS_ACT_L or InputId.HOTBAR_CONTENTS_ACT_R)
		{
			var dutySlot = hotbarModule->GetDutyActionSlot((uint)(inputId - InputId.HOTBAR_CONTENTS_ACT_L));
			return dutySlot != null && dutySlot->IsActive ? &dutySlot->HotbarSlot : null;
		}

		return null;
	}

	private static bool IsRepeatableSlot(RaptureHotbarModule.HotbarSlot* slot)
	{
		if (slot == null || slot->IsEmpty)
		{
			return false;
		}

		return slot->CommandType switch
		{
			RaptureHotbarModule.HotbarSlotType.Action => true,
			RaptureHotbarModule.HotbarSlotType.PetAction => true,
			RaptureHotbarModule.HotbarSlotType.BgcArmyAction => true,
			RaptureHotbarModule.HotbarSlotType.LostFindsItem => true,
			RaptureHotbarModule.HotbarSlotType.PhantomAction => true,
			RaptureHotbarModule.HotbarSlotType.GeneralAction => slot->ApparentActionMode is HotbarSlotApparentActionMode.GeneralActionDutyOrPhantom
				or HotbarSlotApparentActionMode.GeneralActionPhantom && slot->ApparentActionId != 0,
			_ => false,
		};
	}

	private static bool IsSingleActivation(RaptureHotbarModule.HotbarSlot* slot)
	{
		if (slot == null)
		{
			return false;
		}

		if (!TryGetSlotActionId(slot, out var actionId))
		{
			return !Service.Config.HoldToCastAbilities;
		}

		if (!ActionHelper.IsRealGCD(actionId))
		{
			return !Service.Config.HoldToCastAbilities;
		}

		return Array.IndexOf(OncePerHoldGcds, actionId) >= 0;
	}

	private static bool TryGetSlotActionId(RaptureHotbarModule.HotbarSlot* slot, out uint actionId)
	{
		var actionManager = ActionManager.Instance();
		if (slot->CommandType == RaptureHotbarModule.HotbarSlotType.Action)
		{
			var id = slot->CommandId;
			actionId = actionManager != null ? actionManager->GetAdjustedActionId(id) : id;
			return actionId != 0;
		}

		if (slot->CommandType == RaptureHotbarModule.HotbarSlotType.GeneralAction
			&& slot->ApparentActionMode is HotbarSlotApparentActionMode.GeneralActionDutyOrPhantom
				or HotbarSlotApparentActionMode.GeneralActionPhantom
			&& slot->ApparentActionId != 0)
		{
			var id = slot->ApparentActionId;
			actionId = actionManager != null ? actionManager->GetAdjustedActionId(id) : id;
			return actionId != 0;
		}

		if (slot->ApparentSlotType == RaptureHotbarModule.HotbarSlotType.Action && slot->ApparentActionId != 0)
		{
			var id = slot->ApparentActionId;
			actionId = actionManager != null ? actionManager->GetAdjustedActionId(id) : id;
			return actionId != 0;
		}

		if (slot->CommandType == RaptureHotbarModule.HotbarSlotType.PhantomAction && slot->CommandId != 0)
		{
			var id = slot->CommandId;
			actionId = actionManager != null ? actionManager->GetAdjustedActionId(id) : id;
			return actionId != 0;
		}

		actionId = 0;
		return false;
	}

	private static uint ResolveEffectiveSlotActionId(RaptureHotbarModule.HotbarSlot* slot)
	{
		if (!TryGetSlotActionId(slot, out var adjustedActionId))
		{
			return 0;
		}

		var actionManager = ActionManager.Instance();
		if (actionManager != null && RuleBook.Find(DataCenter.Job,
			new InterceptContext((ActionID)adjustedActionId, TargetHelper.InvalidTargetId, ActionManager.UseActionMode.None),
			(_, replacement) => ActionInterceptor.CanUse(actionManager, replacement, TargetHelper.InvalidTargetId)) is { Blocks: false } rule)
		{
			return actionManager->GetAdjustedActionId((uint)rule.Replacement);
		}

		return adjustedActionId;
	}

	private static bool CanRepeat(RaptureHotbarModule.HotbarSlot* slot, int firedCount, bool allowQueueOverride)
	{
		var actionManager = ActionManager.Instance();
		if (actionManager == null)
		{
			return false;
		}

		if (actionManager->AreaTargetingActionId != 0)
		{
			return false;
		}

		if (StatusHelper.PlayerHasStatus(false, ChanneledStatuses))
		{
			return false;
		}

		if (actionManager->ActionQueued && !allowQueueOverride)
		{
			return false;
		}

		if (actionManager->CastActionId != 0 && actionManager->CastTimeTotal - actionManager->CastTimeElapsed > QueueWindowSeconds)
		{
			return false;
		}

		if (actionManager->AnimationLock > QueueWindowSeconds)
		{
			return false;
		}

		if (slot == null)
		{
			return true;
		}

		if (!IsRepeatableSlot(slot))
		{
			return false;
		}

		if (firedCount > 0 && IsSingleActivation(slot))
		{
			return false;
		}

		if (TryGetSlotActionId(slot, out var adjustedActionId))
		{
			if (RuleBook.Find(DataCenter.Job, new InterceptContext((ActionID)adjustedActionId, TargetHelper.InvalidTargetId, ActionManager.UseActionMode.None),
				(_, replacement) => ActionInterceptor.CanUse(actionManager, replacement, TargetHelper.InvalidTargetId)) is { } rule)
			{
				if (rule.Blocks)
				{
					return false;
				}

				var replacement = actionManager->GetAdjustedActionId((uint)rule.Replacement);
				if (replacement != adjustedActionId)
				{
					adjustedActionId = replacement;
				}
			}

			if (ActionHelper.IsRealGCD(adjustedActionId))
			{
				var gcdDetail = actionManager->GetRecastGroupDetail(ActionHelper.GCDCooldownGroup - 1);
				if (gcdDetail != null && gcdDetail->IsActive && gcdDetail->Total - gcdDetail->Elapsed > QueueWindowSeconds)
				{
					return false;
				}
			}

			if (actionManager->GetCurrentCharges(adjustedActionId) == 0)
			{
				var group = actionManager->GetRecastGroup(1, adjustedActionId);
				var detail = actionManager->GetRecastGroupDetail(group);
				if (detail != null && detail->IsActive && detail->Total - detail->Elapsed > QueueWindowSeconds)
				{
					return false;
				}

				var additionalGroup = actionManager->GetAdditionalRecastGroup(ActionType.Action, adjustedActionId);
				if (additionalGroup >= 0 && additionalGroup != group)
				{
					var additionalDetail = actionManager->GetRecastGroupDetail(additionalGroup);
					if (additionalDetail != null && additionalDetail->IsActive && additionalDetail->Total - additionalDetail->Elapsed > QueueWindowSeconds)
					{
						return false;
					}
				}
			}

			if (actionManager->GetActionStatus(ActionType.Action, adjustedActionId, TargetHelper.InvalidTargetId, false, false) != (uint)ActionStatus.None)
			{
				return false;
			}

			if (!actionManager->IsActionTargetInRange(ActionType.Action, adjustedActionId))
			{
				return false;
			}
		}

		return true;
	}
}
