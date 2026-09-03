namespace EducaWebApi.Domain.Responses
{
    public class PaginacaoBase
    {
        public int? PaginaAtual { get; set; }
        public int? TamanhoPagina { get; set; }
        public int? TotalRegistros { get; set; }
        public int? TotalPaginas { get; set; }
    }
}
