using Challenge.LargeFileSort.Structures;
using System.Runtime.CompilerServices;

namespace Challenge.LargeFileSort.Sorter
{
	/// <summary>
	/// Helper methods for file chunk sorting operations.
	/// </summary>
	public static class FileChunkSorterHelpers
	{
		/// <summary>
		/// Finds the median of three elements in the array for use as a pivot in QuickSort.
		/// Compares the first, middle, and last elements to select a median.
		/// </summary>
		/// <param name="lines">The array of LineInfo objects.</param>
		/// <param name="low">The starting index of the range.</param>
		/// <param name="high">The ending index of the range.</param>
		/// <returns>The index of the median element.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int MedianOfThree(LineInfo[] lines, int low, int high)
		{
			int mid = low + high >> 1;
			if (CompareLines(in lines[low], in lines[mid]) > 0) Swap(lines, low, mid);
			if (CompareLines(in lines[low], in lines[high]) > 0) Swap(lines, low, high);
			if (CompareLines(in lines[mid], in lines[high]) > 0) Swap(lines, mid, high);
			return mid;
		}

		/// <summary>
		/// Partitions the array around a pivot, ensuring all elements smaller than the pivot
		/// are on its left and all elements larger are on its right.
		/// </summary>
		/// <param name="lines">The array of LineInfo objects.</param>
		/// <param name="low">The starting index of the range.</param>
		/// <param name="high">The ending index of the range.</param>
		/// <param name="pivotIndex">The index of the pivot element.</param>
		/// <returns>The final position of the pivot.</returns>
		public static int Partition(LineInfo[] lines, int low, int high, int pivotIndex)
		{
			var pivot = lines[pivotIndex];
			Swap(lines, pivotIndex, high);
			int i = low - 1;
			for (int j = low; j < high; j++)
			{
				if (CompareLines(in lines[j], in pivot) <= 0)
				{
					i++;
					Swap(lines, i, j);
				}
			}

			Swap(lines, i + 1, high);
			return i + 1;
		}

		/// <summary>
		/// Swaps two elements in the array.
		/// </summary>
		/// <param name="lines">The array of LineInfo objects.</param>
		/// <param name="i">The index of the first element.</param>
		/// <param name="j">The index of the second element.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Swap(LineInfo[] lines, int i, int j)
		{
			(lines[j], lines[i]) = (lines[i], lines[j]);
		}

		/// <summary>
		/// Compares two LineInfo objects based on their cached text and numeric values.
		/// </summary>
		/// <param name="a">The first LineInfo object.</param>
		/// <param name="b">The second LineInfo object.</param>
		/// <returns>
		/// A negative value if a is less than b, zero if they are equal, or a positive value if a is greater than b.
		/// </returns>
		public static int CompareLines(in LineInfo a, in LineInfo b)
		{
			// Use CachedText directly for comparisons
			int cmp = string.Compare(a.CachedText, b.CachedText, StringComparison.Ordinal);
			if (cmp != 0) return cmp;

			return a.Number.CompareTo(b.Number);
		}
	}
}
