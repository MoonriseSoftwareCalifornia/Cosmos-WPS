using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cosmos.Cms.Models
{
    /// <summary>
    /// Users in a Role
    /// </summary>
    public class UsersInRoleViewModel
    {
        /// <summary>
        /// Role Id
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        /// Role Name
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// New user Ids
        /// </summary>
        [Required(ErrorMessage = "Please select at least one.")]
        [Display(Name = "New Users for Role")]
        public string[] UserIds { get; set; }
        /// <summary>
        /// User list
        /// </summary>
        public List<SelectedUserViewModel> Users { get; set; } = new List<SelectedUserViewModel>();
    }

    /// <summary>
    /// Selected user view model
    /// </summary>
    public class SelectedUserViewModel
    {
        /// <summary>
        /// User Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// User email address
        /// </summary>
        public string Email { get; set; }
    }
}
