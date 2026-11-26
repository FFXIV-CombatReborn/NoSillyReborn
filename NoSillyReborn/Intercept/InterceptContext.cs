using FFXIVClientStructs.FFXIV.Client.Game;

namespace NoSillyReborn.Intercept;

internal readonly struct InterceptContext(ActionID action, ulong targetId, ActionManager.UseActionMode mode)
{
	public ActionID Action { get; } = action;

	public ulong TargetId { get; } = targetId;

	public ActionManager.UseActionMode Mode { get; } = mode;

	public IGameObject? Target => TargetHelper.Resolve(TargetId);

	public IGameObject? EffectiveTarget => TargetHelper.ResolveForAction(Action, TargetId);
}
