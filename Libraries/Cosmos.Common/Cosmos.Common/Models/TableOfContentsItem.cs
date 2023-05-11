using System;
using System.Collections.Generic;

namespace Cosmos.Common.Models
{
    public class TableOfContents
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNo { get; set; }
        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Total number of items.
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// Items in the current page
        /// </summary>
        public List<TableOfContentsItem> Items { get; set; }
    }

    /// <summary>
    /// Table of Contents (TOC) Item
    /// </summary>
    public class TableOfContentsItem
    {
        /// <summary>
        /// URL Path to page
        /// </summary>
        public string UrlPath { get; set; }

        /// <summary>
        /// Title of page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Published date and time
        /// </summary>
        public DateTimeOffset Published { get; set; }

        /// <summary>
        /// When last updated
        /// </summary>
        public DateTimeOffset Updated { get; set; }

        /// <summary>
        /// Banner or preview image
        /// </summary>
        public string BannerImage { get; set; }
        /// <summary>
        /// Author name
        /// </summary>
        public string AuthorInfo { get; set; }
    }
}
