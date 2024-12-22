using Challenge.LargeFileSort.Constants;
using Challenge.LargeFileSort.Splitter;
using System.Collections.Concurrent;

namespace Challenge.LargeFileSort
{
	class FileSorter
	{
		private const int DefaultChankSize = 128;

		static void Main(string[] args)
		{
			string inputFile, outputFile, algo;
			int chunkSize;
			int maxDegreeOfParallelism = Environment.ProcessorCount;

			if (args.Length < 4)
			{
				Console.WriteLine("Not enough parameters provided. Please input them manually.");

				Console.Write("Enter the path to the input file: ");
				inputFile = Console.ReadLine() ?? string.Empty;

				Console.Write("Enter the path to the output file: ");
				outputFile = Console.ReadLine() ?? string.Empty;

				Console.Write("Enter the chunk size in MB: ");
				string chunkSizeString = Console.ReadLine() ?? string.Empty;

				Console.Write($"Enter the algorithm ({AlgoType.Timsort} or {AlgoType.QuickSort}): ");
				algo = Console.ReadLine() ?? string.Empty;

				Console.Write("Enter the maximum number of threads to use (optional, press Enter to use default - 128 MB): ");
				string? threadsInput = Console.ReadLine();

				if (!int.TryParse(chunkSizeString, out chunkSize))
				{
					chunkSize = DefaultChankSize;
					LogHelper.EnqueueLog($"Invalid or missing chunk size. Defaulting to {DefaultChankSize} MB.", true);
				}

				if (!string.IsNullOrWhiteSpace(threadsInput) && int.TryParse(threadsInput, out var maxThreads) && maxThreads > 0)
				{
					maxDegreeOfParallelism = maxThreads;
				}
				else
				{
					LogHelper.EnqueueLog($"Invalid or missing thread count. Defaulting to {maxDegreeOfParallelism} threads.", true);
				}
			}
			else
			{
				inputFile = args[0];
				outputFile = args[1];
				algo = args[2];

				if (!int.TryParse(args[3], out chunkSize))
				{
					chunkSize = DefaultChankSize;
					LogHelper.EnqueueLog($"Invalid or missing chunk size argument. Defaulting to {DefaultChankSize} MB.", true);
				}

				if (args.Length > 4 && int.TryParse(args[4], out var maxThreads) && maxThreads > 0)
				{
					maxDegreeOfParallelism = maxThreads;
				}
				else
				{
					LogHelper.EnqueueLog($"Invalid or missing thread count argument. Defaulting to {maxDegreeOfParallelism} threads.", true);
				}
			}

			if (!File.Exists(inputFile))
			{
				LogHelper.EnqueueLog($"Input file '{inputFile}' not found. Exiting program.", true);
				return;
			}

			// Validate and log selected algorithm
			if (algo != AlgoType.Timsort && algo != AlgoType.QuickSort)
			{
				algo = AlgoType.QuickSort;
				LogHelper.EnqueueLog($"Invalid algorithm selected. Defaulting to {AlgoType.QuickSort}.", true);
			}
			LogHelper.EnqueueLog($"Selected algorithm: {algo}", true);

			var parallelOptions = new ParallelOptions
			{
				MaxDegreeOfParallelism = maxDegreeOfParallelism
			};

			LogHelper.EnqueueLog("Starting chunk-based sorting...", true);

			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			// Split chunks
			LogHelper.EnqueueLog("Split chunks started.", true);
			var splitWatch = System.Diagnostics.Stopwatch.StartNew();
			List<string> chunkFiles = FileSorterHelpers.SplitIntoLineAlignedChunks(inputFile, chunkSize * 1024 * 1024);
			splitWatch.Stop();
			LogHelper.EnqueueLog($"Split chunks completed in {splitWatch.Elapsed.TotalSeconds:F2} seconds.", true);

			// Sort chunks in parallel
			Parallel.ForEach(chunkFiles, parallelOptions, (chunkFile, state, index) =>
			{
				var chunkWatch = System.Diagnostics.Stopwatch.StartNew();
				LogHelper.EnqueueLog($"Chunk {index + 1} started.", true);

				// Storing in temp user files
				string sortedChunk = chunkFile + ".sorted";
				Sorter.FileChunkSorter.SortChunk(chunkFile, sortedChunk, algo);
				chunkWatch.Stop();

				LogHelper.EnqueueLog($"Chunk {index + 1} completed in {chunkWatch.Elapsed.TotalSeconds:F2} seconds.", true);
			});

			// Merge sorted chunks
			LogHelper.EnqueueLog("Merge chunks started.", true);
			var mergeWatch = System.Diagnostics.Stopwatch.StartNew();
			var sortedChunkFiles = chunkFiles.Select(cf => cf + ".sorted").ToList();
			FileSorterHelpers.MergeSortedChunks(sortedChunkFiles, outputFile);
			mergeWatch.Stop();
			LogHelper.EnqueueLog($"Merge chunks completed in {mergeWatch.Elapsed.TotalSeconds:F2} seconds.", true);

			// Delete temp files
			foreach (var f in chunkFiles.Concat(sortedChunkFiles))
			{
				try { File.Delete(f); } catch { }
			}

			stopwatch.Stop();
			double fileSizeMB = new FileInfo(outputFile).Length / (1024.0 * 1024.0);

			string successMessage = $"All chunks sorted and merged. Duration: {stopwatch.Elapsed.TotalSeconds:F2} seconds. Output file size: {fileSizeMB:F2} MB.";
			LogHelper.EnqueueLog(successMessage, true);
		}
	}

	// Helper class for logging
	public static class LogHelper
	{
		private static readonly string logFile = "sort_log.txt";
		private static readonly ConcurrentQueue<string> logQueue = new();
		private static readonly object fileLock = new();

		public static void EnqueueLog(string message, bool writeToConsole = false)
		{
			string logMessage = $"[{DateTime.Now}] INFO: {message}\n";
			logQueue.Enqueue(logMessage);
			if (writeToConsole)
			{
				Console.WriteLine(logMessage);
			}
			FlushLogs();
		}

		private static void FlushLogs()
		{
			lock (fileLock)
			{
				while (logQueue.TryDequeue(out var logMessage))
				{
					File.AppendAllText(logFile, logMessage);
				}
			}
		}
	}
}
