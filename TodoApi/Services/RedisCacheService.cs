using StackExchange.Redis;

namespace TodoApi.Services
{
    /// <summary>
    /// Сервис для работы с Redis, реализующий удаление ключей по шаблону.
    /// </summary>
    public class RedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="RedisCacheService"/>.
        /// Получает подключение к Redis через <see cref="IConnectionMultiplexer"/>.
        /// </summary>
        /// <param name="redis">Объект для подключения и взаимодейсвия с Redis.</param>
        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        /// <summary>
        /// Асинхронно удаляет все ключи из Redis, соответствующие заданному паттерну.
        /// </summary>
        /// <param name="pattern">Шаблон ключа для поиска (например, "tasks-*").</param>
        /// <returns>Задача, представляющая асинхронную операцию удаления ключей.</returns>
        public async Task RemoveKeysByPatternAsync(string pattern)
        {
            var endpoints = _redis.GetEndPoints();

            foreach(var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern);

                foreach(var key in keys)
                {
                    await _db.KeyDeleteAsync(key);
                }
            }
        }
    }
}
