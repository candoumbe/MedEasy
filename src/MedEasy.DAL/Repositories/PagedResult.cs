using System.Collections.Generic;

namespace MedEasy.DAL.Repositories
{
    public class PagedResult<T>
    {

        public IEnumerable<T> Entries { get; private set; }

        public int Total { get; private set; }

        public PagedResult(IEnumerable<T> entries, int total)
        {
            Entries = entries;
            Total = total;
        }
    }
}