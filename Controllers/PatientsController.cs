using HospitalManagementApi.Models;
using HospitalManagementApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApi.Controllers
{
	// Repository Pattern + Dependency Injection
	[ApiController]
	[Route("api/[controller]")]
	public class PatientsController : ControllerBase
	{
		private readonly IPatientRepository _repo;

		public PatientsController(IPatientRepository repo)
		{
			_repo = repo;
		}

		// GET all patients from Azure SQL
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var patients = await _repo.GetAllAsync(); // async - thread not blocked
				return Ok(patients); 
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Server error: {ex.Message}"); // app never crashes
			}
		}

		// GET single patient — 404 if not found
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			try
			{
				var patient = await _repo.GetByIdAsync(id);
				if (patient == null)
					return NotFound("Patient not found!"); 
				return Ok(patient); 
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Server error: {ex.Message}");
			}
		}

		// POST — DTO carries data, saves to Azure SQL
		[HttpPost]
		public async Task<IActionResult> Add([FromBody] CreatePatientDto dto)
		{
			try
			{
				var patient = new Patient // DTO → Patient object
				{
					FullName = dto.FullName,
					Email = dto.Email,
					Phone = dto.Phone,
					DateOfBirth = dto.DateOfBirth
				};
				await _repo.AddAsync(patient); // parameterized query — no SQL Injection
				return Ok("Patient added successfully!"); 
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while adding patient: {ex.Message}");
			}
		}

		// DELETE patient by ID
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _repo.DeleteAsync(id);
				return Ok("Patient deleted successfully!"); 
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while deleting patient: {ex.Message}");
			}
		}
	}
}