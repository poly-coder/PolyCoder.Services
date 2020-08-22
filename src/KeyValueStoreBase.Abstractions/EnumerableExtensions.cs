using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolyCoder.Services.KeyValueStoreBase.Abstractions
{
    // TODO: Move to DotNetX
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Windowed<T>(this IEnumerable<T> source, int windowSize)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (windowSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(source), "Window size must be greater than 0");
            }

            var list = new List<T>();

            foreach (var item in source)
            {
                list.Add(item);

                if (list.Count >= windowSize)
                {
                    yield return list.AsEnumerable();

                    list = new List<T>();
                }
            }

            if (list.Count >= 0)
            {
                yield return list.AsEnumerable();
            }
        }
    }
}
