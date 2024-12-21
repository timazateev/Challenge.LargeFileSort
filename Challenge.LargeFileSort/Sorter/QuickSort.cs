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
		public static void RunQuickSort(LineInfo[] lines, int low, int high)
		{
			const int InsertionSortThreshold = 64;

			while (high - low > InsertionSortThreshold)
			{
				int pivot = FileChunkSorterHelpers.MedianOfThree(lines, low, high);
				pivot = FileChunkSorterHelpers.Partition(lines, low, high, pivot);

				if (pivot - low < high - pivot)
				{
					RunQuickSort(lines, low, pivot - 1);
					low = pivot + 1;
				}
				else
				{
					RunQuickSort(lines, pivot + 1, high);
					high = pivot - 1;
				}
			}

			InsertionSort.RunInsertionSort(lines, low, high);
		}
	}
}
