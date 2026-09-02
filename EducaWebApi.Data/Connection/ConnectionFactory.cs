using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EducaWebApi.Data.Connection
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class SqlConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["EducaWebApiConnection"].ConnectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
