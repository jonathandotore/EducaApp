using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace EducaWebApi.Models
{
    public class MainResponse<T> : PaginacaoBase
    {
        public T Dados { get; set; }
        public bool ContemErros { get; set; }
        public string Mensagem { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public static MainResponse<T> Sucesso(T dados, HttpStatusCode statusCode = HttpStatusCode.OK, string mensagem = null)
        {
            return new MainResponse<T>
            {
                Dados = dados,
                ContemErros = false,
                Mensagem = mensagem,
                StatusCode = statusCode
            };
        }

        public static MainResponse<T> Erro(string mensagem, HttpStatusCode statusCode)
        {
            return new MainResponse<T>
            {
                Dados = default(T),
                ContemErros = true,
                Mensagem = mensagem,
                StatusCode = statusCode
            };
        }

        public MainResponse<T> ComPaginacao(int paginaAtual, int tamanhoPagina, int totalRegistros)
        {
            PaginaAtual = paginaAtual;
            TamanhoPagina = tamanhoPagina;
            TotalRegistros = totalRegistros;
            TotalPaginas = tamanhoPagina > 0
                ? (int)System.Math.Ceiling(totalRegistros / (double)tamanhoPagina)
                : 0;
            return this;
        }
    }
}