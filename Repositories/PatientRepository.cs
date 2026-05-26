using Microsoft.Data.SqlClient;
using HospitalManagementApi.Models;

namespace HospitalManagementApi.Repositories
{
	// inherits BaseRepository (connection) + implements IPatientRepository (contract)
	public class PatientRepository : BaseRepository, IPatientRepository
	{
		public PatientRepository(IConfiguration config) : base(config) { }

		// GET all patients — SELECT * FROM Patients
		public async Task<IEnumerable<Patient>> GetAllAsync()
		{
			var patients = new List<Patient>(); // collection to hold multiple patients
			try
			{
				using var conn = await GetOpenConnectionAsync(); // connection from BaseRepository
				var cmd = new SqlCommand("SELECT * FROM Patients", conn);
				var reader = await cmd.ExecuteReaderAsync(); // reads multiple rows
				while (await reader.ReadAsync()) // loop through each row
				{
					patients.Add(new Patient
					{
						PatientId = (int)reader["PatientId"],
						FullName = reader["FullName"].ToString(),
						Email = reader["Email"].ToString(),
						Phone = reader["Phone"].ToString(),
						DateOfBirth = DateOnly.FromDateTime((DateTime)reader["DateOfBirth"])
					});
				}
			}
			catch (SqlException ex)
			{
				throw new Exception($"Database error while fetching patients: {ex.Message}");
			}
			return patients;
		}

		// GET single patient by ID — parameterized query prevents SQL Injection
		public async Task<Patient?> GetByIdAsync(int id)
		{
			try
			{
				using var conn = await GetOpenConnectionAsync();
				var cmd = new SqlCommand(
					"SELECT * FROM Patients WHERE PatientId = @Id", conn);
				cmd.Parameters.AddWithValue("@Id", id); // parameterized — no SQL Injection
				var reader = await cmd.ExecuteReaderAsync();
				if (await reader.ReadAsync()) // single row
				{
					return new Patient
					{
						PatientId = (int)reader["PatientId"],
						FullName = reader["FullName"].ToString(),
						Email = reader["Email"].ToString(),
						Phone = reader["Phone"].ToString(),
						DateOfBirth = DateOnly.FromDateTime((DateTime)reader["DateOfBirth"])
					};
				}
			}
			catch (SqlException ex)
			{
				throw new Exception($"Database error while fetching patient by ID: {ex.Message}");
			}
			return null; 
		}

		// GET only email — used in appointment booking (auto fetch)
		public async Task<string?> GetEmailByIdAsync(int id)
		{
			try
			{
				using var conn = await GetOpenConnectionAsync();
				var cmd = new SqlCommand(
					"SELECT Email FROM Patients WHERE PatientId = @Id", conn);
				cmd.Parameters.AddWithValue("@Id", id);
				var result = await cmd.ExecuteScalarAsync(); // returns single value
				return result?.ToString();
			}
			catch (SqlException ex)
			{
				throw new Exception($"Database error while fetching patient email: {ex.Message}");
			}
		}

		// INSERT new patient — parameterized query, no SQL Injection
		public async Task AddAsync(Patient patient)
		{
			try
			{
				using var conn = await GetOpenConnectionAsync();
				var cmd = new SqlCommand(
					@"INSERT INTO Patients (FullName, Email, Phone, DateOfBirth)
                      VALUES (@FullName, @Email, @Phone, @DateOfBirth)", conn);
				cmd.Parameters.AddWithValue("@FullName", patient.FullName);
				cmd.Parameters.AddWithValue("@Email", patient.Email);
				cmd.Parameters.AddWithValue("@Phone", patient.Phone);
				cmd.Parameters.AddWithValue("@DateOfBirth",
					patient.DateOfBirth.ToDateTime(TimeOnly.MinValue));
				await cmd.ExecuteNonQueryAsync(); // no data returned — just executes
			}
			catch (SqlException ex)
			{
				throw new Exception($"Database error while adding patient: {ex.Message}");
			}
		}

		// DELETE patient by ID — ExecuteNonQuery — no data returned
		public async Task DeleteAsync(int id)
		{
			try
			{
				using var conn = await GetOpenConnectionAsync();
				var cmd = new SqlCommand(
					"DELETE FROM Patients WHERE PatientId = @Id", conn);
				cmd.Parameters.AddWithValue("@Id", id);
				await cmd.ExecuteNonQueryAsync(); // no data coming back from DB
			}
			catch (SqlException ex)
			{
				throw new Exception($"Database error while deleting patient: {ex.Message}");
			}
		}
	}
}