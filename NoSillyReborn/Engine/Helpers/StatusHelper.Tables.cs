namespace NoSillyReborn.Engine.Helpers;

public static partial class StatusHelper
{
	public static StatusID[] RangePhysicalDefense { get; } =
	[
		StatusID.Troubadour,
		StatusID.Tactician_1951,
		StatusID.Tactician_2177,
		StatusID.ShieldSamba,
	];

	public static StatusID[] PendingRaiseStatus { get; } =
	[
		StatusID.Raise,
		StatusID.Raise_1140,
	];
}
