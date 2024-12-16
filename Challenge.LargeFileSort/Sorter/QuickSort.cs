using Challenge.LargeFileSort.Structures;

namespace Challenge.LargeFileSort.Sorter
{
	public static class QuickSort
	{
		/// <summary>
		// QuickSort with median-of-three
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="low"></param>
		/// <param name="high"></param>
		/// <param name="span"></param>
		public static void RunQuickSort(LineInfo[] lines, int low, int high, Span<byte> span)
		{
			const int InsertionSortThreshold = 64;

			while (high - low > InsertionSortThreshold)
			{
				int pivot = FileChunkSorterHelpers.MedianOfThree(lines, low, high, span);
				pivot = FileChunkSorterHelpers.Partition(lines, low, high, pivot, span);

				// Recursively sort the smaller part and leave the larger part in the loop to reduce the stack depth
				if (pivot - low < high - pivot)
				{
					RunQuickSort(lines, low, pivot - 1, span);
					low = pivot + 1;
				}
				else
				{
					RunQuickSort(lines, pivot + 1, high, span);
					high = pivot - 1;
				}
			}

			// For small subarrays we use insertion sort
			InsertionSort.RunInsertionSort(lines, low, high, span);
		}
	}
}
