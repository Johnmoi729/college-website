using CollegeWebsite.Models;
using MongoDB.Driver;

namespace CollegeWebsite.Services
{
    public class DepartmentService : MongoDBService<Department>, IMongoDBService<Department>
    {
        public DepartmentService(DatabaseSettings settings)
            : base(settings, settings.DepartmentsCollectionName)
        {
        }

        public async Task<Department?> GetByDepartmentCodeAsync(string departmentCode)
        {
            return await FindAsync(d => d.DepartmentCode == departmentCode).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task AddFacultyToDepartmentAsync(string departmentId, string facultyId)
        {
            var department = await GetByIdAsync(departmentId);
            if (department != null && !department.FacultyIds!.Contains(facultyId))
            {
                department.FacultyIds!.Add(facultyId);
                await UpdateAsync(departmentId, department);
            }
        }

        public async Task RemoveFacultyFromDepartmentAsync(string departmentId, string facultyId)
        {
            var department = await GetByIdAsync(departmentId);
            if (department != null && department.FacultyIds!.Contains(facultyId))
            {
                department.FacultyIds!.Remove(facultyId);
                await UpdateAsync(departmentId, department);
            }
        }

        public async Task AddCourseToDepartmentAsync(string departmentId, string courseId)
        {
            var department = await GetByIdAsync(departmentId);
            if (department != null && !department.CourseIds!.Contains(courseId))
            {
                department.CourseIds!.Add(courseId);
                await UpdateAsync(departmentId, department);
            }
        }

        public async Task RemoveCourseFromDepartmentAsync(string departmentId, string courseId)
        {
            var department = await GetByIdAsync(departmentId);
            if (department != null && department.CourseIds!.Contains(courseId))
            {
                department.CourseIds!.Remove(courseId);
                await UpdateAsync(departmentId, department);
            }
        }
    }
}