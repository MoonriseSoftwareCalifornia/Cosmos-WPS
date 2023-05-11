using System;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Grid pager partial view model
    /// </summary>
    public class GridPagerPartialViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="rowCount"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentSort"></param>
        /// <param name="getUrl"></param>
        /// <param name="filter"></param>
        /// <param name="selectOne"></param>
        /// <param name="showThumbnails"></param>
        public GridPagerPartialViewModel(int pageNumber, int rowCount, int pageSize, string sortOrder, string currentSort, string getUrl, string filter = "", bool selectOne = false, bool showThumbnails = false)
        {
            // Set values as-is
            RowCount = rowCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SortOrder = sortOrder;
            CurrentSort = currentSort;
            GetUrl = getUrl;

            // Calculations
            var at = System.Convert.ToDouble(RowCount) / PageSize;
            PageCount = System.Convert.ToInt32(Math.Ceiling(at));
            PreviousPage = Math.Max(0, PageNumber - 1);
            LastPage = PageCount - 1;
            NextPage = Math.Min(pageNumber + 1, LastPage);
            Filter = filter;
            SelectOne = selectOne;
            ShowThumbnails = showThumbnails;
        }
        /// <summary>
        /// Total number of rows
        /// </summary>
        public int RowCount { get; }

        /// <summary>
        /// Current page number (zero index)
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Number of rows in a page
        /// </summary>
        public int PageSize { get; }
        /// <summary>
        /// Previous page
        /// </summary>
        public int PreviousPage { get; }
        /// <summary>
        /// Next page
        /// </summary>
        public int NextPage { get; }
        /// <summary>
        /// Last page
        /// </summary>
        public int LastPage { get; }
        /// <summary>
        /// Page count
        /// </summary>
        public int PageCount { get; }
        /// <summary>
        /// Sort order
        /// </summary>
        public string SortOrder { get; }
        /// <summary>
        /// Current sort item
        /// </summary>
        public string CurrentSort { get; }

        /// <summary>
        /// Get method URL
        /// </summary>
        public string GetUrl { get; }

        /// <summary>
        /// Filter value
        /// </summary>
        public string Filter { get; }
        /// <summary>
        /// Only allow one file to be selected at a time
        /// </summary>
        public bool SelectOne { get; }
        /// <summary>
        /// Show image thumbnails
        /// </summary>
        public bool ShowThumbnails { get; }
    }
}
