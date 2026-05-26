using HospitalManagementApi.Models;
using HospitalManagementApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementApi.Controllers
{
	// Repository Pattern + Dependency Injection
	[ApiController]
	[Route("api/[controller]")]
	public class DoctorsController : ControllerBase
	{
		private readonly IDoctorRepository _repo;

		// DI — IDoctorRepository injected automatically from Program.cs
		public DoctorsController(IDoctorRepository repo)
		{
			_repo = repo;
		}

		// GET all doctors — 200 OK
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var doctors = await _repo.GetAllAsync(); // async — thread not blocked
				return Ok(doctors); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Server error: {ex.Message}");
			}
		}

		// GET only available doctors — IsAvailable = true
		[HttpGet("available")]
		public async Task<IActionResult> GetAvailable()
		{
			try
			{
				var doctors = await _repo.GetAvailableAsync();
				return Ok(doctors); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Server error: {ex.Message}");
			}
		}

		// POST — Add doctor with availability slots
		// DoctorId not needed — DB auto generates it
		[HttpPost]
		public async Task<IActionResult> Add([FromBody] CreateDoctorDto dto)
		{
			try
			{
				// DTO → Doctor object
				// IsAvailable, AvailableFrom, AvailableTo — used in appointment validation
				var doctor = new Doctor
				{
					FullName = dto.FullName,
					Specialization = dto.Specialization,
					IsAvailable = dto.IsAvailable,
					AvailableFrom = dto.AvailableFrom,
					AvailableTo = dto.AvailableTo
				};
				await _repo.AddAsync(doctor); // saves to Azure SQL
				return Ok("Doctor added successfully!"); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while adding doctor: {ex.Message}");
			}
		}

		// PUT — Update doctor details or availability
		[HttpPut]
		public async Task<IActionResult> Update([FromBody] Doctor doctor)
		{
			try
			{
				await _repo.UpdateAsync(doctor); // updates in Azure SQL
				return Ok("Doctor updated successfully!"); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while updating doctor: {ex.Message}");
			}
		}

		// DELETE — 404 if not found, else delete
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var doctor = await _repo.GetByIdAsync(id);
				if (doctor == null)
					return NotFound("Doctor not found!"); // 404

				await _repo.DeleteAsync(id);
				return Ok("Doctor deleted successfully!"); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while deleting doctor: {ex.Message}");
			}
		}
	}
}