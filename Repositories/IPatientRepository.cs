using HospitalManagementApi.Models;

namespace HospitalManagementApi.Repositories
{
	// Interface — Repository Pattern
	// Controller talks to this interface, not direct class
	// easy to change implementation without touching Controller
	public interface IPatientRepository
	{
		Task<IEnumerable<Patient>> GetAllAsync();   
		Task<Patient?> GetByIdAsync(int id);         
		Task<string?> GetEmailByIdAsync(int id);     
		Task AddAsync(Patient patient);             
		Task DeleteAsync(int id);                    
	}
}