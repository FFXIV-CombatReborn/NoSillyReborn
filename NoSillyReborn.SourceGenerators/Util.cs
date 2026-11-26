namespace NoSillyReborn.SourceGenerators
{
	internal static class Util
	{
		/// <summary>
		/// Indents each line of the string with four spaces.
		/// </summary>
		/// <param name="str">The input string.</param>
		/// <returns>The indented string.</returns>
		public static string Table(this string str) => "    " + str.Replace("\n", "\n    ");
	}
}
