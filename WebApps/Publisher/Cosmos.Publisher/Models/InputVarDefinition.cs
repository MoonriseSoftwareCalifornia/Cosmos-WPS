namespace Cosmos.Cms.Publisher.Models
{

    /// <summary>
    /// Input variable definition
    /// </summary>
    public class InputVarDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="definition"></param>
        /// <example>InputVarDefinition("firstName:string:64")</example>
        public InputVarDefinition(string definition)
        {
            var parts = definition.Split(':');
            Name = parts[0];
            if (parts.Length > 1)
            {
                MaxLength = int.Parse(parts[1]);
            }
            else
            {
                MaxLength = 256;
            }
        }

        /// <summary>
        /// Input variable name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Maximum number of string characters
        /// </summary>
        public int MaxLength { get; set; } = 256;
    }
}
