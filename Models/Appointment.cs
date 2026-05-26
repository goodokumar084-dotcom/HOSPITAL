namespace HospitalManagementApi.Models
{
	// Model class — maps to Appointments table in Azure SQL
	public class Appointment
	{
		public int AppointmentId { get; set; }  
		public int PatientId { get; set; }       
		public int DoctorId { get; set; }        
		public DateTime AppointmentDate { get; set; } 
		public string? Status { get; set; }      
		public string? PatientEmail { get; set; } 
	}
}