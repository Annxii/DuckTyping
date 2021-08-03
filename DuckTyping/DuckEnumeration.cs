namespace DuckTyping
{
    class DuckEnumeration
    {
        public class DuckEnumerator
        {
            private int count;
            private int idx = 0;
            public DuckEnumerator(int count)
            {
                this.count = count;
            }

            public bool MoveNext()
            {
                idx++;
                return idx < count;
            }

            public string Current
            {
                get { return new string('x', idx); }
            }
        }

        public DuckEnumerator GetEnumerator() => new DuckEnumerator(10);
    }
}
