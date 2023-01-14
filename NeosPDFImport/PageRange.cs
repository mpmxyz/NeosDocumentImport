using System;
using System.Collections;
using System.Collections.Generic;

namespace NeosDocumentImport
{
    public class PageRange : IEnumerable<int>
    {
        private readonly int from;
        private readonly int to;

        public int Count
        {
            get
            {
                return Math.Abs(from - to) + 1;
            }
        }

        public PageRange(int from, int to)
        {
            this.from = from;
            this.to = to;
        }
        public PageRange(int x) : this(x, x)
        {

        }

        public PageRange Clamp(int min, int max)
        {
            if ((from < min && to < min) || (from > max && to > max))
            {
                return null;
            }
            return new PageRange(Clamp(min, from, max), Clamp(min, to, max));
        }

        private int Clamp(int min, int x, int max)
        {
            return Math.Min(Math.Max(x, min), max);
        }

        public IEnumerator<int> GetEnumerator()
        {
            if (from < to)
            {
                for (int i = from; i <= to; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = from; i >= to; i--)
                {
                    yield return i;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
