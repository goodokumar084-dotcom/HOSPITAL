using Azure.Identity;
using HospitalManagementApi.Repositories;

namespace HospitalManagementApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Key Vault URL
			var keyVaultUrl = "https://vikashkey1.vault.azure.net/";

			// Managed Identity
			var credential = new ManagedIdentityCredential();
			builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);

			// Register controllers
			builder.Services.AddControllers();

			// Dependency Injection
			builder.Services.AddScoped<IPatientRepository, PatientRepository>();
			builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
			builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
			builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();

			// Swagger
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			app.MapGet("/", () => "Welcome to Hospital Management API!"); // default route
			app.UseSwagger();   // enable Swagger
			app.UseSwaggerUI(); // enable Swagger UI in browser
			app.UseAuthorization();
			app.MapControllers(); // map all controller routes
			app.Run();
		}
	}
}