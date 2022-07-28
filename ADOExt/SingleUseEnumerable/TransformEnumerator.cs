using System;
using System.Collections;
using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public class TransformEnumerator<T, TSource> : IEnumerator<T>
    {
        private readonly IEnumerator<TSource> source;
        private readonly Func<TSource, T> transform;

        public TransformEnumerator(IEnumerator<TSource> source, Func<TSource, T> transform)
        {
            this.source = source;
            this.transform = transform;
        }

        public T Current => transform(source.Current);

        object IEnumerator.Current => transform(source.Current);

        public void Dispose()
        {
            source.Dispose();
        }

        public bool MoveNext()
        {
            return source.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
