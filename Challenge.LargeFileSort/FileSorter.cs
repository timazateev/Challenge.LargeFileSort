using Challenge.LargeFileSort.Constants;
using Challenge.LargeFileSort.Splitter;
using System.Collections.Concurrent;

namespace Challenge.LargeFileSort
{
	class FileSorter
	{
		static void Main(string[] args)
		{
			Console.Write("Enter the path to the input file: ");
			string inputFile = Console.ReadLine() ?? string.Empty;

			Console.Write("Enter the path to the output file: ");
			string outputFile = Console.ReadLine() ?? string.Empty;

			Console.Write("Enter the chunk size in Mb: ");
			string chunkSizeString = Console.ReadLine() ?? string.Empty;

			Console.Write($"Enter the algorithm ({AlgoType.Timsort} or {AlgoType.QuickSort}): ");
			string algo = Console.ReadLine() ?? string.Empty;

			int maxDegreeOfParallelism = Environment.ProcessorCount;
			Console.WriteLine("Enter the maximum number of threads to use: ");
			if (int.TryParse(Console.ReadLine(), out var maxThreads) && maxThreads > 0)
			{
				maxDegreeOfParallelism = maxThreads;
			}

			var parallelOptions = new ParallelOptions
			{
				MaxDegreeOfParallelism = maxDegreeOfParallelism
			};

			if (!int.TryParse(chunkSizeString, out var chunkSize))
			{
				chunkSize = 100;
			}

			string logFile = "sort_log.txt";
			var logQueue = new ConcurrentQueue<string>();

			if (!File.Exists(inputFile))
			{
				string errorMessage = $"Input file '{inputFile}' not found.";
				Console.WriteLine(errorMessage);
				File.AppendAllText(logFile, $"[{DateTime.Now}] ERROR: {errorMessage}\n");
				return;
			}

			logQueue.Enqueue($"[{DateTime.Now}] INFO: Starting chunk-based sorting...\n");
			Console.WriteLine("Starting chunk-based sorting...");

			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			// Split chunks
			logQueue.Enqueue($"[{DateTime.Now}] INFO: Split chunks started.\n");
			Console.WriteLine($"[{DateTime.Now}] INFO: Split chunks started.\n"); //TODO: add log class for logging in one invoke
			var splitWatch = System.Diagnostics.Stopwatch.StartNew();
			List<string> chunkFiles = FileSorterHelpers.SplitIntoLineAlignedChunks(inputFile, chunkSize * 1024 * 1024);
			splitWatch.Stop();
			logQueue.Enqueue($"[{DateTime.Now}] INFO: Split chunks completed in {splitWatch.Elapsed.TotalSeconds:F2} seconds.\n");
			Console.WriteLine($"[{DateTime.Now}] INFO: Split chunks completed in {splitWatch.Elapsed.TotalSeconds:F2} seconds.\n");

			// Sort chunks in parallel
			Parallel.ForEach(chunkFiles, parallelOptions, (chunkFile, state, index) =>
			{
				var chunkWatch = System.Diagnostics.Stopwatch.StartNew();
				Console.WriteLine($"[{DateTime.Now}] INFO: Chunk {index + 1} started.\n");

				// Storing in temp user files
				string sortedChunk = chunkFile + ".sorted";
				Sorter.FileChunkSorter.SortChunk(chunkFile, sortedChunk, algo);
				chunkWatch.Stop();

				Console.WriteLine($"Chunk {index + 1} sort complete in {chunkWatch.Elapsed.TotalSeconds:F2} seconds.");
				logQueue.Enqueue($"[{DateTime.Now}] INFO: Chunk {index + 1} completed in {chunkWatch.Elapsed.TotalSeconds:F2} seconds.\n");
			});

			// Merge sorted chunks
			logQueue.Enqueue($"[{DateTime.Now}] INFO: Merge chunks started.\n");
			Console.WriteLine($"[{DateTime.Now}] INFO: Merge chunks started.\n");
			var mergeWatch = System.Diagnostics.Stopwatch.StartNew();
			var sortedChunkFiles = chunkFiles.Select(cf => cf + ".sorted").ToList();
			FileSorterHelpers.MergeSortedChunks(sortedChunkFiles, outputFile);
			mergeWatch.Stop();
			Console.WriteLine($"[{DateTime.Now}] INFO: Merge chunks completed in {mergeWatch.Elapsed.TotalSeconds:F2} seconds.\n");
			logQueue.Enqueue($"[{DateTime.Now}] INFO: Merge chunks completed in {mergeWatch.Elapsed.TotalSeconds:F2} seconds.\n");

			// Delete temp files
			foreach (var f in chunkFiles.Concat(sortedChunkFiles))
			{
				try { File.Delete(f); } catch { }
			}

			stopwatch.Stop();
			double fileSizeMB = new FileInfo(outputFile).Length / (1024.0 * 1024.0);

			string successMessage = $"All chunks sorted and merged. Duration: {stopwatch.Elapsed.TotalSeconds:F2} seconds. Output file size: {fileSizeMB:F2} MB.";
			Console.WriteLine(successMessage);
			logQueue.Enqueue($"[{DateTime.Now}] INFO: {successMessage}\n");

			// Flush log queue to file
			File.AppendAllLines(logFile, logQueue);
		}
	}
}