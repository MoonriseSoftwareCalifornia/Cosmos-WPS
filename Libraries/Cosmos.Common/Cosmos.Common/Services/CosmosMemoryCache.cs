using Microsoft.Extensions.Caching.Memory;

namespace Cosmos.Cms.Common.Services
{
    /// <summary>
    /// Short term memory cache to take pressure off of Redis and SQL
    /// </summary>
    /// <remarks>See: <seealso href="https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-5.0#use-setsize-size-and-sizelimit-to-limit-cache-size"/> (Microsoft documentation)</remarks>
    public class CosmosMemoryCache
    {
        public MemoryCache Cache { get; set; }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="bytesmaxsize">Maximum number of bytes to use for cache (default is 64 mb)</param>
        public CosmosMemoryCache(long bytesmaxsize = 64000000)
        {
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = bytesmaxsize
            });
        }
    }
}
