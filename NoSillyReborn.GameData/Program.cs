using Lumina;
using Lumina.Data;
using Newtonsoft.Json.Linq;
using NoSillyReborn.GameData.Getters;
using NoSillyReborn.GameData.Getters.Actions;
using System.Net;
using System.Resources.NetStandard;

namespace NoSillyReborn.GameData
{

	/// <summary>
	/// Entry point for the NoSillyReborn GameData program.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Main method to execute the program.
		/// </summary>
		public static async Task Main()
		{
			try
			{
				var gameData = new Lumina.GameData(@"C:\FF14\game\sqpack", new LuminaOptions
				{
					LoadMultithreaded = true,
					CacheFileResources = true,
					PanicOnSheetChecksumMismatch = false,
					DefaultExcelLanguage = Language.English,
				});

				var assemblyLocation = typeof(Program).Assembly.Location;
				Console.WriteLine($"Assembly location: {assemblyLocation}");

				var solutionRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation)!, "..", "..", "..", "..", ".."));
				Console.WriteLine($"Solution root: {solutionRoot}");

				var propertiesDir = Path.Combine(solutionRoot, "NoSillyReborn.SourceGenerators", "Properties");
				Console.WriteLine($"Properties directory: {propertiesDir}");
				Console.WriteLine($"Properties directory exists: {Directory.Exists(propertiesDir)}");

				void WriteResource(string name, string content)
				{
					var path = Path.Combine(propertiesDir, $"{name}.resx");
					Console.WriteLine($"Writing resource: {path}");
					using var writer = new ResXResourceWriter(path);
					writer.AddResource("Value", content);
					writer.Generate();
				}

				WriteResource("Status", new StatusGetter(gameData).GetCode());
				WriteResource("ContentType", new ContentTypeGetter(gameData).GetCode());
				WriteResource("ActionId", new ActionIdGetter(gameData).GetCode());
				WriteResource("ActionCategory", new ActionCategoryGetter(gameData).GetCode());
				WriteResource("BNpcName", new BNpcNameGetter(gameData).GetCode());
				WriteResource("TraitId", new TraitIdGetter(gameData).GetCode());
				WriteResource("GameMetadata", new GameMetadataGetter(gameData).GetCode());

				try
				{
					using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
					var result = await client.GetAsync("https://raw.githubusercontent.com/karashiiro/FFXIVOpcodes/master/opcodes.json");

					if (result.StatusCode == HttpStatusCode.OK)
					{
						var responseStream = await result.Content.ReadAsStringAsync();

						var root = JToken.Parse(responseStream)[0]!["lists"]!;
						var strs = new List<string>();
						foreach (var level1 in root.Children())
						{
							foreach (var level2 in level1.Children())
							{
								foreach (var tok in level2.Children())
								{
									if (tok is JObject i)
									{
										var name = ((JValue)i["name"]!).Value as string;
										var description = name!.Space();

										strs.Add($$"""
                    /// <summary>
                    ///{{description}}
                    /// </summary>
                    [Description("{{description}}")]
                    {{name}} = {{((JValue)i["opcode"]!).Value}},
                    """);
									}
								}
							}
						}
						WriteResource("OpCode", string.Join("\n", strs));
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Skipping OpCode update: {ex.Message}");
				}

				Console.WriteLine("Finished!");
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"An error occurred: {ex.Message}");
			}
		}
	}
}
