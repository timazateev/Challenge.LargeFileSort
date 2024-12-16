namespace Challenge.LargeFileSort.Splitter
{
	/// <summary>
	/// Priority key, compare by text, then by number
	/// </summary>
	/// <param name="text"></param>
	/// <param name="number"></param>
	struct PriorityKey(string text, long number) : IComparable<PriorityKey>
    {
        public string Text = text;
        public long Number = number;

		public readonly int CompareTo(PriorityKey other)
        {
            int cmp = string.Compare(Text, other.Text, StringComparison.Ordinal);
            if (cmp != 0) return cmp;
            return Number.CompareTo(other.Number);
        }
    }
}