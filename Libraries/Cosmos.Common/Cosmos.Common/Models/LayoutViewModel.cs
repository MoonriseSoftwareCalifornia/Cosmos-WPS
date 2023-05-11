using Cosmos.Common.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Cosmos.Common.Models
{
    /// <summary>
    ///     VSiew model used on layout list page
    /// </summary>
    [Serializable]
    public class LayoutViewModel
    {
        public LayoutViewModel()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="layout"></param>
        public LayoutViewModel(Layout layout)
        {
            if (layout != null)
            {
                Id = layout.Id;
                LayoutName = layout.LayoutName;
                Notes = layout.Notes;
                Head = layout.Head;
                HtmlHeader = layout.HtmlHeader;
                FooterHtmlContent = layout.FooterHtmlContent;
            }
        }

        /// <summary>
        ///     Identity key of the entity
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        ///     Indicates if this is the default layout of the site
        /// </summary>
        [Display(Name = "Is default layout?")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        ///     Friendly name of layout
        /// </summary>
        [Display(Name = "Layout Name")]
        [StringLength(128)]
        public string LayoutName { get; set; }

        /// <summary>
        ///     Notes regarding this layout
        /// </summary>
        [Display(Name = "Notes")]
        [DataType(DataType.Html)]
        public string Notes { get; set; }

        /// <summary>
        ///     Content injected into the head tag
        /// </summary>
        [Display(Name = "HEAD Content")]
        [DataType(DataType.Html)]
        public string Head { get; set; }

        /// <summary>
        ///     Content injected into page header
        /// </summary>
        [Display(Name = "Header Html Content", GroupName = "Header")]
        [DataType(DataType.Html)]
        public string HtmlHeader { get; set; }

        /// <summary>
        ///     Content injected into the page footer
        /// </summary>
        [Display(Name = "Footer Html Content", GroupName = "Footer")]
        [DataType(DataType.Html)]
        public string FooterHtmlContent { get; set; }

        /// <summary>
        ///     Gets a detached entity.
        /// </summary>
        /// <returns></returns>
        public Layout GetLayout(bool decode = false)
        {
            if (decode)
                return new Layout
                {
                    Id = Id,
                    IsDefault = IsDefault,
                    LayoutName = LayoutName,
                    Notes = HttpUtility.HtmlDecode(Notes),
                    Head = HttpUtility.HtmlDecode(Head),
                    HtmlHeader = HttpUtility.HtmlDecode(HtmlHeader),
                    FooterHtmlContent = HttpUtility.HtmlDecode(FooterHtmlContent)
                };
            return new Layout
            {
                Id = Id,
                IsDefault = IsDefault,
                LayoutName = LayoutName,
                Notes = Notes,
                Head = Head,
                HtmlHeader = HtmlHeader,
                FooterHtmlContent = FooterHtmlContent
            };
        }
    }
}