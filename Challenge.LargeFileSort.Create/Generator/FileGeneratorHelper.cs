using System.Diagnostics;
using System.Text;

namespace Challenge.LargeFileSort.Create.Generator
{
	public static class FileGeneratorHelper
	{
		public static void GenerateTestFile(string filePath, long targetSizeBytes)
		{
			string logFilePath = filePath + ".log";

			if (File.Exists(logFilePath))
			{
				File.Delete(logFilePath);
			}

			using var logFileStream = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			using var logWriter = new StreamWriter(logFileStream, Encoding.UTF8, 65536);

			void Log(string message)
			{
				Console.WriteLine(message);
				logWriter.WriteLine(message);
				logWriter.Flush();
			}

			Stopwatch stopwatch = Stopwatch.StartNew();
			DateTime startTime = DateTime.UtcNow;
			Log($"Generation started at {startTime}.");

			int frequentCount = 100;
			int rareCount = 900;

			var frequentStrings = Enumerable.Range(1, frequentCount)
											.Select(i => $"CommonString_{i}")
											.ToArray();

			var rareStrings = Enumerable.Range(1, rareCount)
										.Select(i => $"RareString_{i}")
										.ToArray();

			string[] prefixWords =
			[
				"Apple", "Banana", "Cherry", "Date", "Elderberry",
				"Fig", "Grape", "Honeydew", "Apricot", "Blackberry",
				"Coconut", "Dragonfruit", "Orange", "Papaya", "Mango",
				"Peach", "Plum", "Raspberry", "Strawberry", "Watermelon"
			];

			string[] extraWords =
			[
				"is", "very", "quite", "extremely", "somewhat", "truly", "really",
				"nice", "tasty", "fresh", "ripe", "amazing", "delicious", "sweet"
			];

			Random rnd = new();
			long maxNumber = 1_000_000_000;

			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536))
			using (var writer = new StreamWriter(fs, Encoding.UTF8, 65536))
			{
				long currentSize = 0;

				while (currentSize < targetSizeBytes)
				{
					long number = (long)(rnd.NextDouble() * maxNumber) + 1;

					string chosenBase = rnd.NextDouble() < 0.8
						? frequentStrings[rnd.Next(frequentStrings.Length)]
						: rareStrings[rnd.Next(rareStrings.Length)];

					string prefix = prefixWords[rnd.Next(prefixWords.Length)];

					int extraCount = rnd.Next(0, 4);
					var chosenExtraWords = Enumerable.Range(0, extraCount)
													.Select(_ => extraWords[rnd.Next(extraWords.Length)])
													.ToArray();

					string line = $"{number}. {prefix} {chosenBase}";
					if (chosenExtraWords.Length > 0)
					{
						line += " " + string.Join(" ", chosenExtraWords);
					}
					line += "\n";

					byte[] lineBytes = Encoding.UTF8.GetBytes(line);

					if (currentSize + lineBytes.Length > targetSizeBytes)
					{
						break;
					}

					fs.Write(lineBytes, 0, lineBytes.Length);
					currentSize += lineBytes.Length;
				}
			}

			stopwatch.Stop();
			DateTime endTime = DateTime.UtcNow;
			TimeSpan elapsed = stopwatch.Elapsed;

			Log($"Generation finished at {endTime}. Elapsed: {elapsed}.");
			Log("Generation done!");
		}
	}
}
