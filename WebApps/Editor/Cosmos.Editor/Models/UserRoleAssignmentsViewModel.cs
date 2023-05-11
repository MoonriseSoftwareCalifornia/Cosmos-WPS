using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// User role assignments view model
    /// </summary>
    public class UserRoleAssignmentsViewModel
    {
        /// <summary>
        /// User Id
        /// </summary>
        [Display(Name = "User Id")]
        public string Id { get; set; }
        /// <summary>
        /// User Email Address
        /// </summary>
        [Display(Name = "User Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// Role assignments for this user
        /// </summary>
        [Display(Name = "Role Assignments")]
        public string[] RoleIds { get; set; }
    }
}
