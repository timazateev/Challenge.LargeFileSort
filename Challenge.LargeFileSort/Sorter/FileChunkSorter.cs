using Challenge.LargeFileSort.Constants;
using Challenge.LargeFileSort.Structures;
using System.Runtime.CompilerServices;

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
					QuickSort.RunQuickSort(lines, 0, lines.Length - 1, span);
					break;
				case AlgoType.Timsort:
					Timsort.RunTimsort(lines, span);
					break;
				default:
					QuickSort.RunQuickSort(lines, 0, lines.Length - 1, span);
					break;
			}
			
			WriteSortedLines(outputFile, span, lines);
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
						lines[lineIndex].LineStart = lineStart;
						lines[lineIndex].LineLength = lineLen;

						// Parse number and get TextStart
						int dotIndex = span.Slice(lineStart, lineLen).IndexOf((byte)'.');
						
						// if there is no dot
						if (dotIndex < 0)
						{
							continue;
							//lines[lineIndex].Number = 0;
							//lines[lineIndex].TextStart = lineStart + lineLen;
						}
						else
						{
							// Number before dot
							var numberSpan = span.Slice(lineStart, dotIndex);
							lines[lineIndex].Number = ParseLongFromAscii(numberSpan);

							// Text starts after dot if exists
							int textPos = lineStart + dotIndex + 2; // always dot and whitespace?
							if (textPos > lineStart + lineLen)
								textPos = lineStart + lineLen;

							lines[lineIndex].TextStart = textPos;
						}

						lineIndex++;
					}
					lineStart = i + 1;
				}
			}
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
		/// 
		/// </summary>
		/// <param name="outputFile"></param>
		/// <param name="span"></param>
		/// <param name="lines"></param>
		private static void WriteSortedLines(string outputFile, Span<byte> span, LineInfo[] lines)
		{
			// Larger buffer?
			using var outFs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.SequentialScan);
			foreach (var line in lines)
			{
				var lineSpan = span.Slice(line.LineStart, line.LineLength);
				outFs.Write(lineSpan);
				outFs.WriteByte((byte)'\n');
			}
		}
	}
}