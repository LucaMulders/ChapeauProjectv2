using Microsoft.Data.SqlClient;

namespace ChapeauProject.Repositories
{
    public abstract class RepositoryBase
    {
        protected readonly string _connectionString;

        protected RepositoryBase(string connectionString)
        {
            _connectionString = connectionString;
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
