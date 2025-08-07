using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;

namespace MagicEastern.ADOExt.Paging
{
    public abstract class PagingRecords<TRecord> : PagingParameters, IEnumerable<TRecord>
    {
        public IEnumerable<TRecord> Records;

        public IEnumerator<TRecord> GetEnumerator()
        {
            return Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Records.GetEnumerator();
        }

        public abstract void PrepareVM(IPrincipal user);
    }
}