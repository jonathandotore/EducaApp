using System;

namespace EducaWebApi.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string mensagem) : base(mensagem) { }
    }
}