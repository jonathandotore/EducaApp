using System;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface ICacheService
    {
        Task<T> ObterAsync<T>(string chave) where T : class;
        Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao) where T : class;
        Task RemoverAsync(string chave);
    }
}
