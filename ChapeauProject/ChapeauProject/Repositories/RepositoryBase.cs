using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ChapeauProject.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly string? _connectionString;

        protected RepositoryBase(IConfiguration configuration)
        {
            // get the (database) connection string from appsettings
            _connectionString = configuration.GetConnectionString("ChapeauProject");
        }

        protected SqlConnection GetConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (SqlException ex)
            {
                throw new Exception("Could not open a connection to the database.", ex);
            }
        }
    }
}
