using NyayaSetu.Models;

namespace NyayaSetu.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllAsync(bool activeOnly = false);
        Task<Department?> GetByIdAsync(int id);
        Task<Department> CreateAsync(Department department);
        Task<bool> UpdateAsync(Department department);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<List<ApplicationCategory>> GetCategoriesAsync(bool activeOnly = true);
    }
}
