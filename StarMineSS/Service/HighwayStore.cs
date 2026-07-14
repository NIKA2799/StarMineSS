using Microsoft.Extensions.Caching.Memory;
using StarMineSS.Model;

namespace StarMineSS.Service
{
    public class HighwayStore(IMemoryCache cache)
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromHours(2);

        public void Save(HighwayGame game)
        {
            ArgumentNullException.ThrowIfNull(game);

            cache.Set(game.Id, game, new MemoryCacheEntryOptions
            {
                SlidingExpiration = Lifetime,
                Size = 1
            });
        }

        public HighwayGame? Get(Guid id) =>
            cache.TryGetValue(id, out HighwayGame? game) ? game : null;

        public void Remove(Guid id) => cache.Remove(id);
    }
}