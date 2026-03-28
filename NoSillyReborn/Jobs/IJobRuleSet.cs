using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs;

/// <summary>
/// Implemented by each job handler to supply a set of <see cref="ActionRule"/>s
/// that the <see cref="ActionInterceptManager"/> evaluates on every player input.
/// </summary>
public interface IJobRuleSet
{
    /// <summary>The ClassJob row ID this rule-set applies to.</summary>
    JobID JobId { get; }

    /// <summary>
    /// Returns all intercept rules for this job.
    /// Rules are evaluated in order; the first matching rule wins.
    /// </summary>
    IReadOnlyList<ActionRule> Rules { get; }
}
