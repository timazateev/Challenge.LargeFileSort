using Challenge.LargeFileSort.Constants;
using Challenge.LargeFileSort.Structures;
using System.Runtime.CompilerServices;
using System.Text;

namespace Challenge.LargeFileSort.Sorter
{
	public static class FileChunkSorter
	{
		public static void SortChunk(string inputFile, string outputFile, string algo)
		{
			long fileSize = new FileInfo(inputFile).Length;
			if (fileSize > int.MaxValue)
			{
				Console.WriteLine("File is too large for this example (limited by int.MaxValue).");
				return;
			}

			// Read file to the memory
			byte[] buffer = File.ReadAllBytes(inputFile);
			Span<byte> span = buffer.AsSpan();

			int lineCount = CountLines(span);
			var lines = new LineInfo[lineCount];

			ExtractLines(span, lines);

			switch (algo)
			{
				case AlgoType.QuickSort:
					QuickSort.RunQuickSort(lines, 0, lines.Length - 1);
					break;
				case AlgoType.Timsort:
					Timsort.RunTimsort(lines);
					break;
				default:
					QuickSort.RunQuickSort(lines, 0, lines.Length - 1);
					break;
			}

			WriteSortedLines(outputFile, lines);
		}


		private static int CountLines(Span<byte> span)
		{
			int lineCount = 0;
			for (int i = 0; i < span.Length; i++)
			{
				if (span[i] == (byte)'\n')
					lineCount++;
			}
			return lineCount;
		}

		private static void ExtractLines(Span<byte> span, LineInfo[] lines)
		{
			int lineIndex = 0;
			int lineStart = 0;

			for (int i = 0; i < span.Length; i++)
			{
				if (span[i] == (byte)'\n')
				{
					int lineEnd = i;
					int lineLen = lineEnd - lineStart;

					if (lineLen > 0 && lineIndex < lines.Length)
					{
						int dotIndex = span.Slice(lineStart, lineLen).IndexOf((byte)'.');

						if (dotIndex >= 0)
						{
							// Parse the number
							lines[lineIndex].Number = ParseLongFromAscii(span.Slice(lineStart, dotIndex));

							// Parse and cache the string
							int textStart = lineStart + dotIndex + 2;
							if (textStart < lineEnd)
							{
								lines[lineIndex].CachedText = ParseStringFromAscii(span.Slice(textStart, lineEnd - textStart));
							}
						}

						lineIndex++;
					}

					lineStart = i + 1;
				}
			}
		}


		/// <summary>
		/// Parses a string from a span of ASCII bytes.
		/// </summary>
		/// <param name="strSpan">The span containing ASCII bytes.</param>
		/// <returns>The parsed string, or null if the span is empty.</returns>
		private static string? ParseStringFromAscii(Span<byte> strSpan)
		{
			if (strSpan.IsEmpty)
			{
				return null;
			}

			return Encoding.ASCII.GetString(strSpan);
		}

		/// <summary>
		/// Parse number from ASCII without allocation
		/// </summary>
		/// <param name="span"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long ParseLongFromAscii(ReadOnlySpan<byte> span)
		{
			long result = 0;
			foreach (byte b in span)
			{
				if (b < '0' || b > '9') break;
				result = result * 10 + (b - (byte)'0');
			}
			return result;
		}

		/// <summary>
		/// Writes sorted lines to the output file using cached text from LineInfo.
		/// </summary>
		/// <param name="outputFile">Path to the output file.</param>
		/// <param name="lines">Sorted array of LineInfo.</param>
		private static void WriteSortedLines(string outputFile, LineInfo[] lines)
		{
			// Use a larger buffer for better write performance
			using var outFs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.SequentialScan);
			using var writer = new StreamWriter(outFs, Encoding.ASCII, 1024 * 1024); // Use a StreamWriter for string writing

			foreach (var line in lines)
			{
				if (!string.IsNullOrEmpty(line.CachedText))
				{
					writer.WriteLine($"{line.Number}. {line.CachedText}");
				}
			}
		}

	}
}