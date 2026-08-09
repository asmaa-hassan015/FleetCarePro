using Microsoft.EntityFrameworkCore;

namespace FleetCarePro.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }

        public int TotalPages { get; private set; }

        public PaginatedList(
            List<T> items,
            int count,
            int pageIndex,
            int pageSize)
        {
            PageIndex = pageIndex;

            TotalPages =
                (int)Math.Ceiling(
                    count / (double)pageSize);

            AddRange(items);
        }

        public bool HasPreviousPage =>
            PageIndex > 1;

        public bool HasNextPage =>
            PageIndex < TotalPages;

        public static PaginatedList<T> Create(
            IQueryable<T> source,
            int pageIndex,
            int pageSize)
        {
            var count = source.Count();

            var totalPages =
                (int)Math.Ceiling(
                    count / (double)pageSize);

            // Guard against an invalid pageIndex coming from the
            // query string (e.g. ?pageIndex=0, a negative number,
            // or a page far beyond the last one). Without this,
            // Skip() below could receive a negative offset and
            // throw at runtime.
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            else if (totalPages > 0 && pageIndex > totalPages)
            {
                pageIndex = totalPages;
            }

            var items = source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedList<T>(
                items,
                count,
                pageIndex,
                pageSize);
        }
    }
}