using EducaWebApi.Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EducaWebApi.Data.Cache
{
    public class RedisCacheService : ICacheService
    {
        public async Task<T> ObterAsync<T>(string chave) where T : class
        {
            try
            {
                var valor = await RedisConnectionFactory.ObterDatabase().StringGetAsync(chave);
                return valor.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<T>(valor);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Falha ao ler cache Redis (chave: {chave}): {ex}");
                return null;
            }
        }

        public async Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao) where T : class
        {
            try
            {
                var json = JsonConvert.SerializeObject(valor);
                await RedisConnectionFactory.ObterDatabase().StringSetAsync(chave, json, expiracao);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Falha ao gravar cache Redis (chave: {chave}): {ex}");
            }
        }

        public async Task RemoverAsync(string chave)
        {
            try
            {
                await RedisConnectionFactory.ObterDatabase().KeyDeleteAsync(chave);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Falha ao invalidar cache Redis (chave: {chave}): {ex}");
            }
        }
    }
}
