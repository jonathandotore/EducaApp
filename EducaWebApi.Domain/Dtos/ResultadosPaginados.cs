using System.Collections.Generic;

namespace EducaWebApi.Domain.Dtos
{
    public class ResultadosPaginados<T>
    {
        public List<T> Itens { get; set; }
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
    }
}
