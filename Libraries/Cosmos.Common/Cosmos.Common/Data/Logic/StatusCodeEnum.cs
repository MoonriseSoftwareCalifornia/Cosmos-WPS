namespace Cosmos.Common.Data.Logic
{
    /// <summary>
    ///     Article status code
    /// </summary>
    public enum StatusCodeEnum
    {
        /// <summary>
        ///     Active, able to display if publish data given.
        /// </summary>
        Active = 0,

        /// <summary>
        ///     In active, can be displayed by logged in users
        /// </summary>
        Inactive = 1,

        /// <summary>
        ///     Considered removed, no one can display until status changes.
        /// </summary>
        Deleted = 2,

        /// <summary>
        ///     The article is a redirect.
        /// </summary>
        Redirect = 3
    }
}