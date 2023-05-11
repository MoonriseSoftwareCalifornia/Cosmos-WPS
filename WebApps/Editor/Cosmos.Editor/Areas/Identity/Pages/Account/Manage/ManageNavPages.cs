using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;

namespace Cosmos.Cms.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// Manage navigation pages class
    /// </summary>
    public static class ManageNavPages
    {
        /// <summary>
        /// Index
        /// </summary>
        public static string Index => "Index";
        /// <summary>
        /// Email
        /// </summary>
        public static string Email => "Email";
        /// <summary>
        /// Change password
        /// </summary>
        public static string ChangePassword => "ChangePassword";
        /// <summary>
        /// Download personal data
        /// </summary>
        public static string DownloadPersonalData => "DownloadPersonalData";
        /// <summary>
        /// Delete personal data
        /// </summary>
        public static string DeletePersonalData => "DeletePersonalData";
        /// <summary>
        /// External logins
        /// </summary>
        public static string ExternalLogins => "ExternalLogins";
        /// <summary>
        /// Personal data
        /// </summary>
        public static string PersonalData => "PersonalData";
        /// <summary>
        /// Two factor authentication
        /// </summary>
        public static string TwoFactorAuthentication => "TwoFactorAuthentication";
        /// <summary>
        /// Index
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string IndexNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, Index);
        }
        /// <summary>
        /// Email navigation
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string EmailNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, Email);
        }
        /// <summary>
        /// Change password nav class
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string ChangePasswordNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, ChangePassword);
        }
        /// <summary>
        /// Download personal data nav class
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string DownloadPersonalDataNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, DownloadPersonalData);
        }
        /// <summary>
        /// Delete personal data nav class
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string DeletePersonalDataNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, DeletePersonalData);
        }
        /// <summary>
        /// External logins nav class
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string ExternalLoginsNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, ExternalLogins);
        }
        /// <summary>
        /// Personal data nav class
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string PersonalDataNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, PersonalData);
        }
        /// <summary>
        /// Two factor authentication nav class
        /// </summary>
        /// <param name="viewContext"></param>
        /// <returns></returns>
        public static string TwoFactorAuthenticationNavClass(ViewContext viewContext)
        {
            return PageNavClass(viewContext, TwoFactorAuthentication);
        }
        /// <summary>
        /// Page nav class
        /// </summary>
        /// <param name="viewContext"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                             ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}