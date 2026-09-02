using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace EducaWebApi.Filters
{
    public class TratamentoDeExcecoesFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpStatusCode statusCode;
            string mensagem;

            switch (context.Exception)
            {
                case NotFoundException notFound:
                    statusCode = HttpStatusCode.NotFound;
                    mensagem = notFound.Message;
                break;

                case ConflictException conflict:
                    statusCode = HttpStatusCode.Conflict;
                    mensagem = conflict.Message;
                break;

                case ValidationException validation:
                    statusCode = HttpStatusCode.BadRequest;
                    mensagem = validation.Message;
                break;

                case DatabaseException database:
                    statusCode = HttpStatusCode.InternalServerError;
                    mensagem = "Ocorreu um erro ao acessar o banco de dados.";
                    System.Diagnostics.Trace.TraceError(database.InnerException?.ToString() ?? database.ToString());
                break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    mensagem = "Ocorreu um erro inesperado ao processar a requisicao.";
                break;
            }

            var erro = MainResponse<object>.Erro(mensagem, statusCode);
            context.Response = context.Request.CreateResponse(statusCode, erro);
        }
    }
}