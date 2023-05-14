using BaseX;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NeosDocumentImport
{
    /// <summary>
    /// represents a range of pages numbers
    /// (e.g. "1" or "3-4")
    /// <br/>
    /// Reverse order is permitted. (e.g. from=5, to=3 is pages 5,4,3)
    /// </summary>
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

        private static int Clamp(int min, int x, int max)
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

        /// <summary>
        /// constructs a list of page ranges from a string of comma separated page range codes
        /// <br/>
        /// Each individual page range is either a single number (single page) or two numbers separated by a minus sign.
        /// (multiple pages, inclusive start/end, reverse direction permitted) <br/>
        /// Whitespace is ignored.
        /// </summary>
        /// <param name="pageString">may be null (equals an empty string)</param>
        /// <returns>a list of page ranges if <paramref name="pageString"/> is valid, null otherwise</returns>
        public static List<PageRange> FromString(string pageString)
        {
            var pages = new List<PageRange>();
            if (pageString == null)
            {
                //null => no selection
                return pages;
            }

            pageString = pageString.RemoveWhitespace();

            if (pageString.Length == 0)
            {
                //empty string => no selection
                return pages;
            }

            foreach (var segment in pageString.Split(','))
            {
                var numbers = segment.Split('-');

                switch (numbers.Length)
                {
                    case 1:
                        if (int.TryParse(numbers[0], out int x))
                        {
                            pages.Add(new PageRange(x));
                        }
                        else
                        {
                            //parsing failed
                            return null;
                        }

                        break;
                    case 2:
                        if (int.TryParse(numbers[0], out int a)
                            && int.TryParse(numbers[1], out int b))
                        {
                            var range = new PageRange(a, b);

                            if (range.Count > 9999)
                            {
                                //hard limit # of generated files to prevent endless lockup of Neos
                                return null;
                            }

                            pages.Add(range);
                        }
                        else
                        {
                            //parsing failed
                            return null;
                        }

                        break;
                    default:
                        //too many minus signs
                        return null;
                }
            }
            return pages;
        }
    }
}
