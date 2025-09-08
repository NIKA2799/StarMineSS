using Microsoft.Extensions.Caching.Memory;
using StarMineSS.Model;

namespace StarMineSS.Service
{
    public class GameStore
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan Lifetime = TimeSpan.FromHours(2);

        public GameStore(IMemoryCache cache) => _cache = cache;

        public void Save(GameState game) =>
            _cache.Set(game.Id, game, new MemoryCacheEntryOptions
            {
                SlidingExpiration = Lifetime,
                Size = 1
            });

        public GameState? Get(Guid id) =>
            _cache.TryGetValue(id, out GameState? game) ? game : null;
    }
}