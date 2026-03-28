using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs;

/// <summary>
/// Describes a single action-replacement rule.
/// When the player attempts to use <see cref="OriginalActionId"/> and
/// <see cref="Condition"/> returns <c>true</c>, the input is replaced
/// with <see cref="ReplacementActionId"/> instead.
/// </summary>
public sealed class ActionRule(ActionID original, ActionID? replacement, Func<bool> condition, string description)
{
    /// <summary>The adjusted action ID the player is trying to use.</summary>
    public uint OriginalActionId { get; init; } = (uint)original;

    /// <summary>The action that should be used instead.</summary>
    public uint ReplacementActionId { get; init; } = replacement.HasValue ? (uint)replacement.Value : 0;

    /// <summary>
    /// Condition that must be true for this replacement to fire.
    /// Evaluated at hook time; keep it cheap.
    /// </summary>
    public Func<bool> Condition { get; init; } = condition;

    /// <summary>Human-readable description shown in the UI.</summary>
    public string Description { get; init; } = description;
}
