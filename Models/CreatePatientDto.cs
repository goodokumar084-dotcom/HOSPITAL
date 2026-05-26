namespace HospitalManagementApi.Models
{
	// DTO — Data Transfer Object
	// carries data from UI (Swagger) to Controller
	// PatientId NOT here — DB auto generates it
	public class CreatePatientDto
	{
		public string? FullName { get; set; }   
		public string? Email { get; set; }        
		public string? Phone { get; set; }        
		public DateOnly DateOfBirth { get; set; } 
	}
}