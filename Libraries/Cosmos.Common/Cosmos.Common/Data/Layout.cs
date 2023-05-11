using System;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Common.Data
{
    /// <summary>
    ///     Website layout content.
    /// </summary>
    [Serializable]
    public class Layout
    {
        /// <summary>
        ///     Identity key for this entity.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Display(Name = "Community Layout Id")]
        public string CommunityLayoutId { get; set; }

        /// <summary>
        ///     If true this is the default layout for website.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        ///     Friendly name of layout
        /// </summary>
        [Display(Name = "Layout Name")]
        [StringLength(128)]
        public string LayoutName { get; set; }

        /// <summary>
        ///     Notes about the layout
        /// </summary>
        [Display(Name = "Notes")]
        [DataType(DataType.Html)]
        public string Notes { get; set; }

        /// <summary>
        ///     Content injected into the web page HEAD
        /// </summary>
        [Display(Name = "HEAD Content")]
        [DataType(DataType.Html)]
        public string Head { get; set; }

        /// <summary>
        ///     Body tag attributes
        /// </summary>
        [Display(Name = "BODY Html Attributes", GroupName = "Body")]
        [StringLength(256)]
        public string BodyHtmlAttributes { get; set; }

        /// <summary>
        ///     Web page header content
        /// </summary>
        [Display(Name = "Header Html Content", GroupName = "Header")]
        [DataType(DataType.Html)]
        public string HtmlHeader { get; set; }

        /// <summary>
        ///     Content injected into the web site footer.
        /// </summary>
        [Display(Name = "Footer Html Content", GroupName = "Footer")]
        [DataType(DataType.Html)]
        public string FooterHtmlContent { get; set; }

    }
}