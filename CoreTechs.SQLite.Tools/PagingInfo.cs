using System;

namespace CoreTechs.SQLite.Tools
{
    public class PagingInfo
    {
        private int _page = 1;
        private int _pageSize = 25;
        private int? _totalItems;

        public int Page
        {
            get { return _page; }
            set
            {
                _page = value > LastPage ? LastPage.Value
                    : value < 1 ? 1
                    : value;
            }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value < 1 ? 1 : value; }
        }

        public int? TotalItems
        {
            get { return _totalItems; }
            set { _totalItems = value < 0 ? null : value; }
        }

        public int? LastPage
        {
            get
            {
                if (TotalItems == null)
                    return null;

                double size = PageSize;
                var items = TotalItems.Value;

                return (int)Math.Ceiling(items / size);
            }
        }
    }
}
