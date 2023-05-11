using System.Collections.Generic;

namespace Cosmos.Cms.Data
{
    /// <summary>
    /// Required roles for Cosmos
    /// </summary>
    public static class RequiredIdentityRoles
    {

        /// <summary>
        /// List of roles required for Cosmos to work
        /// </summary>
        public static List<string> Roles
        {
            // Reviewers,Authors,Editors,Administrators
            get
            {
                var roles = new List<string>();
                roles.Add(Administrators);
                roles.Add(Authors);
                roles.Add(Editors);
                roles.Add(Reviewers);

                return roles;
            }
        }

        /// <summary>
        /// Administrators role
        /// </summary>
        public const string Administrators = "Administrators";
        /// <summary>
        /// Authors
        /// </summary>
        public const string Authors = "Authors";
        /// <summary>
        /// Editors
        /// </summary>
        public const string Editors = "Editors";
        /// <summary>
        /// Reviewers
        /// </summary>
        public const string Reviewers = "Reviewers";
    }
}
