using Challenge.LargeFileSort.Structures;

namespace Challenge.LargeFileSort.Sorter
{
	public static class Timsort
	{
		private const int RunLength = 32;

		/// <summary>
		/// Timsort implementation, a hybrid sorting algorithm that combines insertion sort and merge sort.
		/// </summary>
		/// <param name="lines">The array of LineInfo to be sorted.</param>
		/// <param name="span">The span representing the original byte data.</param>
		public static void RunTimsort(LineInfo[] lines, Span<byte> span)
		{
			// Step 1: Sort small runs with insertion sort
			for (int i = 0; i < lines.Length; i += RunLength)
			{
				InsertionSort.RunInsertionSort(lines, i, Math.Min(i + RunLength - 1, lines.Length - 1), span);
			}

			// Step 2: Merge sorted runs
			for (int size = RunLength; size < lines.Length; size *= 2)
			{
				for (int left = 0; left < lines.Length; left += 2 * size)
				{
					int mid = left + size - 1;
					int right = Math.Min(left + 2 * size - 1, lines.Length - 1);

					if (mid < right)
					{
						Merge(lines, left, mid, right, span);
					}
				}
			}
		}

		/// <summary>
		/// Merges two sorted subarrays into a single sorted array.
		/// </summary>
		/// <param name="lines">The array of LineInfo to be merged.</param>
		/// <param name="low">The starting index of the first subarray.</param>
		/// <param name="mid">The ending index of the first subarray.</param>
		/// <param name="high">The ending index of the second subarray.</param>
		/// <param name="span">The span representing the original byte data.</param>
		private static void Merge(LineInfo[] lines, int low, int mid, int high, Span<byte> span)
		{
			int leftSize = mid - low + 1;
			int rightSize = high - mid;

			var left = new LineInfo[leftSize];
			var right = new LineInfo[rightSize];

			Array.Copy(lines, low, left, 0, leftSize);
			Array.Copy(lines, mid + 1, right, 0, rightSize);

			int i = 0, j = 0, k = low;

			while (i < leftSize && j < rightSize)
			{
				if (FileChunkSorterHelpers.CompareLines(in left[i], in right[j], span) <= 0)
				{
					lines[k++] = left[i++];
				}
				else
				{
					lines[k++] = right[j++];
				}
			}

			while (i < leftSize)
			{
				lines[k++] = left[i++];
			}

			while (j < rightSize)
			{
				lines[k++] = right[j++];
			}
		}
	}
}
