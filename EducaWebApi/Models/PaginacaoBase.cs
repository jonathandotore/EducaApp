using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EducaWebApi.Models
{
    public class PaginacaoBase
    {
        public int? PaginaAtual { get; set; }
        public int? TamanhoPagina { get; set; }
        public int? TotalRegistros { get; set; }
        public int? TotalPaginas { get; set; }
    }
}