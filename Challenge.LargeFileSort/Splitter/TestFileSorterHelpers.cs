namespace Challenge.LargeFileSort.Splitter
{
	public static class TestFileSorterHelpers
	{
		/// <summary>
		/// Merges sorted file chunks into a single output file using a priority queue.
		/// Each entry in the queue is prioritized by a custom key that compares both the text and number parts of a line.
		/// </summary>
		/// <param name="sortedChunkFiles">List of sorted chunk file paths.</param>
		/// <param name="outputFile">Path to the output file.</param>
		public static void MergeSortedChunks(List<string> sortedChunkFiles, string outputFile)
		{
			var readers = sortedChunkFiles.Select(f => new StreamReader(f)).ToList();

			try
			{
				using var outFs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);
				using var writer = new StreamWriter(outFs);
				var minHeap = new PriorityQueue<(string line, int sourceIndex), PriorityKey>(readers.Count);

				// Initialize the priority queue with the first line from each file
				for (int i = 0; i < readers.Count; i++)
				{
					string? line = readers[i].ReadLine();
					if (line != null)
					{
						ParseLine(line, out string text, out long num);
						minHeap.Enqueue((line, i), new PriorityKey(text, num));
					}
				}

				// Merge lines from all chunks
				while (minHeap.Count > 0)
				{
					// Extract the smallest element (line and its source file index)
					var (line, srcIndex) = minHeap.Dequeue();

					// Write the line to the output file
					writer.WriteLine(line);

					// Read the next line from the same file and enqueue it
					string? newLine = readers[srcIndex].ReadLine();
					if (newLine != null)
					{
						ParseLine(newLine, out string text, out long num);
						minHeap.Enqueue((newLine, srcIndex), new PriorityKey(text, num));
					}
				}
			}
			finally
			{
				// Ensure all file readers are properly disposed of
				foreach (var r in readers)
					r.Dispose();
			}
		}

		/// <summary>
		/// Parses a line into its text and number parts.
		/// A line is expected to follow the format: "Number. Text".
		/// </summary>
		/// <param name="line">The input line to parse.</param>
		/// <param name="text">The text part extracted from the line.</param>
		/// <param name="number">The numeric part extracted from the line.</param>
		internal static void ParseLine(string line, out string text, out long number)
		{
			int dotPos = line.IndexOf('.');
			if (dotPos < 0)
			{
				text = line;
				number = 0;
				return;
			}

			ReadOnlySpan<char> numPart = line.AsSpan(0, dotPos).Trim();
			if (!long.TryParse(numPart, out number))
				number = 0;

			text = dotPos + 2 < line.Length ? line[(dotPos + 2)..] : "";
		}

		/// <summary>
		/// Splits a large input file into smaller, line-aligned chunks.
		/// Each chunk is approximately the specified target size, ensuring no line is split across chunks.
		/// </summary>
		/// <param name="inputFile">The path to the input file.</param>
		/// <param name="targetChunkSize">The target size (in bytes) for each chunk.</param>
		/// <returns>A list of paths to the generated chunk files.</returns>
		public static List<string> SplitIntoLineAlignedChunks(string inputFile, long targetChunkSize)
		{
			List<string> chunks = [];

			using (var fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
			{
				byte[] buffer = new byte[8192];
				long fileLength = fs.Length;

				while (fs.Position < fileLength)
				{
					string chunkFile = Path.GetTempFileName();
					chunks.Add(chunkFile);

					using var chunkFs = new FileStream(chunkFile, FileMode.Create, FileAccess.Write);
					long bytesInCurrentChunk = 0;
					long initialReadBytes = Math.Min(targetChunkSize, fileLength - fs.Position);
					long readSoFar = 0;
					byte lastWrittenByte = 0; // Tracks the last byte written to handle line alignment

					// Read the main block of data for the chunk
					while (readSoFar < initialReadBytes)
					{
						int toRead = (int)Math.Min(buffer.Length, initialReadBytes - readSoFar);
						int read = fs.Read(buffer, 0, toRead);
						if (read <= 0) break;
						chunkFs.Write(buffer, 0, read);
						readSoFar += read;
						bytesInCurrentChunk += read;
						lastWrittenByte = buffer[read - 1];
					}

					// Handle end-of-file scenarios
					if (fs.Position >= fileLength)
						continue;

					// If the chunk does not end on a newline, continue reading until it does
					if (bytesInCurrentChunk > 0 && lastWrittenByte == (byte)'\n')
						continue;

					// Align to the nearest newline
					bool lineEnded = false;
					while (!lineEnded && fs.Position < fileLength)
					{
						int read = fs.Read(buffer, 0, buffer.Length);
						if (read <= 0) break;

						int newlineIndex = -1;
						for (int i = 0; i < read; i++)
						{
							if (buffer[i] == (byte)'\n')
							{
								newlineIndex = i;
								break;
							}
						}

						if (newlineIndex >= 0)
						{
							// Write up to and including the newline
							chunkFs.Write(buffer, 0, newlineIndex + 1);
							// Seek back the remaining bytes in the buffer
							int leftover = read - (newlineIndex + 1);
							if (leftover > 0)
							{
								fs.Seek(-leftover, SeekOrigin.Current);
							}
							lineEnded = true;
						}
						else
						{
							chunkFs.Write(buffer, 0, read);
						}
					}
				}
			}
			return chunks;
		}
	}
}
