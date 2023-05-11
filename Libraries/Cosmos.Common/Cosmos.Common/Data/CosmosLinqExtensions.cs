using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmos.Common.Data
{
    public static class CosmosLinqExtensions
    {
        public static async Task<bool> CosmosAnyAsync<T>(this IQueryable<T> entities)
        {
            return (await entities.CountAsync()) > 0;
        }
    }
}
