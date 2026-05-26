using Azure.Messaging.ServiceBus;
using HospitalManagementApi.Models;
using HospitalManagementApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HospitalManagementApi.Controllers
{
	// Repository Pattern + DI — 3 repositories + IConfiguration injected
	[ApiController]
	[Route("api/[controller]")]
	public class AppointmentsController : ControllerBase
	{
		private readonly IAppointmentRepository _repo;
		private readonly IDoctorRepository _doctorRepo;
		private readonly IPatientRepository _patientRepo;
		private readonly IConfiguration _config; // fetches secrets from Key Vault

		public AppointmentsController(
			IAppointmentRepository repo,
			IDoctorRepository doctorRepo,
			IPatientRepository patientRepo,
			IConfiguration config)
		{
			_repo = repo;
			_doctorRepo = doctorRepo;
			_patientRepo = patientRepo;
			_config = config;
		}

		// GET all appointments — 200 OK
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var appointments = await _repo.GetAllAsync();
				return Ok(appointments);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Server error: {ex.Message}");
			}
		}

		// GET single appointment — 404 if not found
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			try
			{
				var appointment = await _repo.GetByIdAsync(id);
				if (appointment == null)
					return NotFound("Appointment not found!"); // 404
				return Ok(appointment); // 200 OK
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Server error: {ex.Message}");
			}
		}

		// POST — Book appointment with 4 validations + Service Bus message
		[HttpPost]
		public async Task<IActionResult> Book([FromBody] CreateAppointmentDto dto)
		{
			try
			{
				// Patient exists? else 404
				var patient = await _patientRepo.GetByIdAsync(dto.PatientId);
				if (patient == null)
					return NotFound("Patient not found!");

				// Doctor exists? else 404
				var doctor = await _doctorRepo.GetByIdAsync(dto.DoctorId);
				if (doctor == null)
					return NotFound("Doctor not found!");

				// IsAvailable check else 400
				if (!doctor.IsAvailable)
					return BadRequest("Doctor is not available!");

				// Time slot check else 400
				if (doctor.AvailableFrom.HasValue && doctor.AvailableTo.HasValue)
				{
					var appointmentTime = dto.AppointmentDate.TimeOfDay;
					if (appointmentTime < doctor.AvailableFrom.Value ||
						appointmentTime > doctor.AvailableTo.Value)
					{
						return BadRequest(
							$"Doctor is not available at this time! " +
							$"Available from {doctor.AvailableFrom} to {doctor.AvailableTo}.");
					}
				}

				// Patient email auto fetched from DB
				var patientEmail = await _patientRepo.GetEmailByIdAsync(dto.PatientId);

				// Create Appointment object, Status = Confirmed
				var appointment = new Appointment
				{
					PatientId = dto.PatientId,
					DoctorId = dto.DoctorId,
					AppointmentDate = dto.AppointmentDate,
					Status = "Confirmed",
					PatientEmail = patientEmail
				};

				// Save to Azure SQL Database
				await _repo.AddAsync(appointment);

				// Send message to Service Bus Queue (async decoupled)
				try
				{
					// connection string fetched from Key Vault via IConfiguration
					var connectionString = _config.GetConnectionString("ServiceBus");
					var queueName = _config["ServiceBus:QueueName"];

					await using var client = new ServiceBusClient(connectionString);
					var sender = client.CreateSender(queueName);

					// JSON message — PatientEmail, Date, Status
					var messageBody = JsonSerializer.Serialize(new
					{
						PatientId = appointment.PatientId,
						DoctorId = appointment.DoctorId,
						AppointmentDate = appointment.AppointmentDate,
						Status = "Confirmed",
						PatientEmail = patientEmail
					});

					await sender.SendMessageAsync(new ServiceBusMessage(messageBody));
				}
				catch (Exception sbEx)
				{
					// booking success even if email fails — decoupled
					return Ok($"Appointment booked successfully! " +
							  $"But email notification failed: {sbEx.Message}");
				}

				// 200 OK — booking done + email will be sent via Logic App
				return Ok("Appointment booked successfully! Confirmation email will be sent.");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error while booking appointment: {ex.Message}");
			}
		}
	}
}