using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class SingleUseEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> source;
        private bool used = false;

        public SingleUseEnumerable(IEnumerable<T> source)
        {
            this.source = source;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (used)
            {
                throw new InvalidOperationException("Multiple enumeration is not supported.");
            }
            used = true;
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class SingleUseEnumerableExt {
        public static IEnumerable<T> AsSingleUse<T>(this IEnumerable<T> source) {
            return new SingleUseEnumerable<T>(source);
        }
    }
}
