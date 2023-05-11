using Cosmos.Common.Models;
using Cosmos.Cms.Data.Logic;
using System.Collections.Generic;

namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     <see cref="ArticleEditLogic.Save(HtmlEditorViewModel, string)" /> result.
    /// </summary>
    public class ArticleUpdateResult
    {
        /// <summary>
        /// Server indicates the content was saved successfully in the database
        /// </summary>
        public bool ServerSideSuccess { get; set; }
        /// <summary>
        ///     Updated or Inserted model
        /// </summary>
        public ArticleViewModel Model { get; set; }

        /// <summary>
        ///     Urls that need to be flushed
        /// </summary>
        public List<string> Urls { get; set; } = new List<string>();
    }
}