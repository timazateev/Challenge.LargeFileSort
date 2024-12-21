using Challenge.LargeFileSort.Structures;

namespace Challenge.LargeFileSort.Sorter
{
	/// <summary>
	/// Implements the InsertionSort algorithm for small subarrays.
	/// </summary>
	public static class InsertionSort
	{
		/// <summary>
		/// Sorts a range of LineInfo objects using the InsertionSort algorithm.
		/// </summary>
		/// <param name="lines">The array of LineInfo objects.</param>
		/// <param name="low">The starting index of the range.</param>
		/// <param name="high">The ending index of the range.</param>
		public static void RunInsertionSort(LineInfo[] lines, int low, int high)
		{
			for (int i = low + 1; i <= high; i++)
			{
				var key = lines[i];
				int j = i - 1;
				while (j >= low && FileChunkSorterHelpers.CompareLines(in lines[j], in key) > 0)
				{
					lines[j + 1] = lines[j];
					j--;
				}
				lines[j + 1] = key;
			}
		}
	}
}