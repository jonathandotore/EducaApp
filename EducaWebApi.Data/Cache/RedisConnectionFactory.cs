using StackExchange.Redis;
using System;
using System.Configuration;

namespace EducaWebApi.Data.Cache
{
    public static class RedisConnectionFactory
    {
        private static readonly Lazy<ConnectionMultiplexer> _conexao = new Lazy<ConnectionMultiplexer>(() =>
        {
            var connectionString = ConfigurationManager.AppSettings["RedisConnection"] ?? "localhost:6379,abortConnect=false,connectTimeout=2500,connectRetry=1,syncTimeout=2500";
            return ConnectionMultiplexer.Connect(connectionString);
        });

        public static IDatabase ObterDatabase()
        {
            return _conexao.Value.GetDatabase();
        }
    }
}
