using System;

namespace Cosmos.Cms.Models
{
    /// <summary>
    ///     This is a verification message that an Editor can be reached.
    /// </summary>
    public class SignalVerifyResult
    {
        /// <summary>
        ///     Data that was recieved
        /// </summary>
        public string Echo { get; set; }

        /// <summary>
        ///     DateTime in UTC for echo
        /// </summary>
        public DateTime Stamp { get; set; }
    }
}