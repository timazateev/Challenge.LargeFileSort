using Challenge.LargeFileSort.Structures;

namespace Challenge.LargeFileSort.Sorter
{
	public static class InsertionSort
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="low"></param>
		/// <param name="high"></param>
		/// <param name="span"></param>
		public static void RunInsertionSort(LineInfo[] lines, int low, int high, Span<byte> span)
		{
			for (int i = low + 1; i <= high; i++)
			{
				var key = lines[i];
				int j = i - 1;
				while (j >= low && FileChunkSorterHelpers.CompareLines(in lines[j], in key, span) > 0)
				{
					lines[j + 1] = lines[j];
					j--;
				}
				lines[j + 1] = key;
			}
		}
	}
}