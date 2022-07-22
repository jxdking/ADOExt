using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class TransformEnumerable<T, TSource> : IEnumerable<T>
    {
        private readonly IEnumerable<TSource> source;
        private readonly Func<TSource, T> transform;

        public TransformEnumerable(IEnumerable<TSource> source, Func<TSource, T> transform)
        {
            this.source = source;
            this.transform = transform;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new TransformEnumerator<T, TSource>(source.GetEnumerator(), transform);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TransformEnumerator<T, TSource>(source.GetEnumerator(), transform);
        }
    }

    public static class TransformEnumerableExt {
        public static IEnumerable<T> Transform<T, TSource>(this IEnumerable<TSource> source, Func<TSource, T> transform) {
            return new TransformEnumerable<T, TSource>(source, transform);
        }
    }
}
