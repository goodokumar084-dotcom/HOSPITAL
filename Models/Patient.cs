
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementApi.Models
{
	// Model class — maps to Patients table in Azure SQL
	public class Patient
	{
		public int PatientId { get; set; }     
		public string? FullName { get; set; }   
		public string? Email { get; set; }      
		public string? Phone { get; set; }      
		public DateOnly DateOfBirth { get; set; } 
	}
}