using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Pagination
{
    public class PaginatedList<T>
    {
        [JsonInclude]
        public List<T> Items = new List<T>();
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }
        public List<T> _items { get; set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalCount = count;

            if (count == 0)
            {
                TotalPages = 0;
            }
            else
            {
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            }

            Items.AddRange(items);
        }

        public PaginatedList(int pageIndex, int totalPages, int count, List<T> items) // For PaginatedListConverter and DynamicExampleProvider
        {
            PageIndex = pageIndex;
            TotalPages = totalPages;
            TotalCount = count;

            Items.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex = 0, int pageSize = 10)
        {
            if (pageIndex == 0)
            {
                // Without pagination
                var items = await source.ToListAsync();
                return new PaginatedList<T>(items, items.Count, 1, items.Count);
            }
            else
            {
                // Pagination
                var count = await source.CountAsync();
                var items = await source.Skip((pageIndex - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToListAsync();
                return new PaginatedList<T>(items, count, pageIndex, pageSize);
            }
        }
        public static Task<PaginatedList<T>> CreateAsync<T>(List<T> source, int pageIndex = 0, int pageSize = 10)
        {
            if (pageIndex == 0)
            {
                // Without pagination
                return Task.FromResult(new PaginatedList<T>(source, source.Count, 1, source.Count));
            }
            else
            {
                // Pagination
                var count = source.Count;
                var items = source.Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToList();

                return Task.FromResult(new PaginatedList<T>(items, count, pageIndex, pageSize));
            }
        }

    }
}
