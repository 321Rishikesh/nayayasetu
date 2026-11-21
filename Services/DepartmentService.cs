using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;

namespace NyayaSetu.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public DepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllAsync(bool activeOnly = false)
        {
            var query = _context.Departments
                .Include(d => d.Officers)
                .Include(d => d.Applications)
                .AsQueryable();

            if (activeOnly) query = query.Where(d => d.IsActive);
            return await query.OrderBy(d => d.Name).ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.Officers).ThenInclude(o => o.User)
                .Include(d => d.Applications)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Department> CreateAsync(Department department)
        {
            department.CreatedAt = DateTime.UtcNow;
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<bool> UpdateAsync(Department department)
        {
            var existing = await _context.Departments.FindAsync(department.Id);
            if (existing == null) return false;

            existing.Name = department.Name.Trim();
            existing.Code = department.Code.Trim().ToUpperInvariant();
            existing.Description = department.Description?.Trim();
            existing.HeadOfDepartment = department.HeadOfDepartment?.Trim();
            existing.ContactEmail = department.ContactEmail?.Trim();
            existing.IsActive = department.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Applications)
                .Include(d => d.Officers)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dept == null) return (false, "Department not found.");
            if (dept.Applications.Any() || dept.Officers.Any())
                return (false, "Cannot delete a department that has linked applications or assigned officers. Deactivate it instead.");

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return (true, "Department deleted.");
        }

        public async Task<List<ApplicationCategory>> GetCategoriesAsync(bool activeOnly = true)
        {
            var query = _context.ApplicationCategories.Include(c => c.DefaultDepartment).AsQueryable();
            if (activeOnly) query = query.Where(c => c.IsActive);
            return await query.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
