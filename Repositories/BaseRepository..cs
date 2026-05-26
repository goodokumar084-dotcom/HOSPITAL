using Microsoft.Data.SqlClient;

namespace HospitalManagementApi.Repositories
{
	// Base class — all repositories inherit from this
	// avoids repeating connection code in every repository
	public abstract class BaseRepository
	{
		private readonly string? _connectionString;

		// connection string fetched from Key Vault via IConfiguration
		protected BaseRepository(IConfiguration config)
		{
			_connectionString = config.GetConnectionString("HospitalDb");
		}

		// opens Azure SQL connection — reused by all repositories
		protected async Task<SqlConnection> GetOpenConnectionAsync()
		{
			var conn = new SqlConnection(_connectionString); // ADO.NET connection
			await conn.OpenAsync(); // thread not blocked
			return conn;
		}
	}
}