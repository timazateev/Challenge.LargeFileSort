using Challenge.LargeFileSort.Structures;
using System.Runtime.CompilerServices;

namespace Challenge.LargeFileSort.Sorter
{
	public static class FileChunkSorterHelpers
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="low"></param>
		/// <param name="high"></param>
		/// <param name="span"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int MedianOfThree(LineInfo[] lines, int low, int high, Span<byte> span)
		{
			int mid = low + high >> 1;
			if (CompareLines(in lines[low], in lines[mid], span) > 0) Swap(lines, low, mid);
			if (CompareLines(in lines[low], in lines[high], span) > 0) Swap(lines, low, high);
			if (CompareLines(in lines[mid], in lines[high], span) > 0) Swap(lines, mid, high);
			return mid;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="low"></param>
		/// <param name="high"></param>
		/// <param name="pivotIndex"></param>
		/// <param name="span"></param>
		/// <returns></returns>
		public static int Partition(LineInfo[] lines, int low, int high, int pivotIndex, Span<byte> span)
		{
			var pivot = lines[pivotIndex];
			Swap(lines, pivotIndex, high);
			int i = low - 1;
			for (int j = low; j < high; j++)
			{
				if (CompareLines(in lines[j], in pivot, span) <= 0)
				{
					i++;
					Swap(lines, i, j);
				}
			}

			Swap(lines, i + 1, high);
			return i + 1;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="i"></param>
		/// <param name="j"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Swap(LineInfo[] lines, int i, int j)
		{
			(lines[j], lines[i]) = (lines[i], lines[j]);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="span"></param>
		/// <returns></returns>
		public static int CompareLines(in LineInfo a, in LineInfo b, Span<byte> span)
		{
			// Сначала сравниваем текст
			var aText = span[a.TextStart..(a.LineStart + a.LineLength)];
			var bText = span[b.TextStart..(b.LineStart + b.LineLength)];

			int cmp = CompareAscii(aText, bText);
			if (cmp != 0) return cmp;

			// Если текст одинаковый, сравниваем числа
			return a.Number.CompareTo(b.Number);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CompareAscii(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
		{
			int length = Math.Min(a.Length, b.Length);
			for (int i = 0; i < length; i++)
			{
				int c = a[i].CompareTo(b[i]);
				if (c != 0)
					return c;
			}
			return a.Length.CompareTo(b.Length);
		}
	}
}