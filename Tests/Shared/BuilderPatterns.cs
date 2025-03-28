using Common.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Shared
{
    public class PaginatedListBuilder<T>
    {
        private List<T> _items = new List<T>();
        private int _pageIndex = 1;
        private int _totalCount = 0;
        private int _pageSize = 10;

        public PaginatedListBuilder<T> WithItems(List<T> items)
        {
            _items = items;
            return this;
        }

        public PaginatedListBuilder<T> WithPageIndex(int pageIndex)
        {
            _pageIndex = pageIndex;
            return this;
        }

        public PaginatedListBuilder<T> WithTotalCount(int totalCount)
        {
            _totalCount = totalCount;
            return this;
        }

        public PaginatedListBuilder<T> WithPageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public PaginatedList<T> Build()
        {
            return new PaginatedList<T>(_items, _totalCount, _pageIndex, _pageSize);
        }
    }
}
