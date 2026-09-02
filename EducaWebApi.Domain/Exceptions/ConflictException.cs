using System;

namespace EducaWebApi.Domain.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException(string mensagem) : base(mensagem) { }
    }
}