namespace Challenge.LargeFileSort.Structures
{
	public struct LineInfo
	{
		public int LineStart;    // string start in the file
		public int LineLength;
		public long Number;      // First part Number for sorting
		public int TextStart;    // text part start after "."
	}
}
