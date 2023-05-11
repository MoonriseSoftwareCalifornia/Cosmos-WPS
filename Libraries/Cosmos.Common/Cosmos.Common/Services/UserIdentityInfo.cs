using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Cosmos.Cms.Common.Services
{
    /// <summary>
    /// User identity information
    /// </summary>
    public class UserIdentityInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user"></param>
        public UserIdentityInfo(ClaimsPrincipal user)
        {
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                IsAuthenticated = user.Identity.IsAuthenticated;
                RoleMembership = ((ClaimsIdentity)user.Identity).Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value).ToList();
                UserName = user.Identity.Name;
            }
            else
            {
                IsAuthenticated = false;
                RoleMembership = new List<string>();
                UserName = string.Empty;
            }
        }

        /// <summary>
        /// User is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }
        /// <summary>
        /// User email address
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Role membership
        /// </summary>
        public List<string> RoleMembership { get; set; }

        /// <summary>
        /// Checks to see if a user is in a role.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsInRole(string name)
        {
            return RoleMembership.Contains(name, StringComparer.CurrentCultureIgnoreCase);
        }
    }
}
