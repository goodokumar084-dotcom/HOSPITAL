using Azure.Storage.Blobs;
using HospitalManagementApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApi.Controllers
{
	// Repository Pattern + DI — IMedicalRecordRepository + IConfiguration injected
	[ApiController]
	[Route("api/[controller]")]
	public class MedicalRecordsController : ControllerBase
	{
		private readonly IMedicalRecordRepository _repo;
		private readonly IConfiguration _config; // fetches Storage connection from Key Vault

		public MedicalRecordsController(
			IMedicalRecordRepository repo,
			IConfiguration config)
		{
			_repo = repo;
			_config = config;
		}

		// GET medical records by PatientId
		[HttpGet("patient/{patientId}")]
		public async Task<IActionResult> GetByPatient(int patientId)
		{
			try
			{
				var records = await _repo.GetByPatientIdAsync(patientId); // async — thread not blocked
				return Ok(records); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while fetching records: {ex.Message}");
			}
		}

		// POST — Upload PDF to Azure Blob Storage
		[HttpPost("upload")]
		public async Task<IActionResult> UploadReport(
			IFormFile file, [FromQuery] int patientId)
		{
			try
			{
				// file validation — empty check
				if (file == null || file.Length == 0)
					return BadRequest("File is empty or not selected!"); // 400

				// Storage connection string fetched from Key Vault via IConfiguration
				var connectionString = _config.GetConnectionString("StorageConnection");

				// connect to Azure Blob Storage
				var blobServiceClient = new BlobServiceClient(connectionString);
				var container = blobServiceClient
					.GetBlobContainerClient("medical-reports"); // container name

				// blobName = PatientId + FileName — easy to identify which patient
				var blobName = $"{patientId}-{file.FileName}";

				// upload PDF to Blob Storage — BlobTrigger will auto fire Function App
				await container.UploadBlobAsync(blobName, file.OpenReadStream());

				return Ok($"Report uploaded successfully! File: {blobName}"); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while uploading file: {ex.Message}");
			}
		}
	}
}