using System;

namespace EducaWebApi.Domain.Exceptions
{
    public class DatabaseException : Exception
    {
        public DatabaseException(string mensagem, Exception innerException)
            : base(mensagem, innerException)
        {
        }
    }
}
