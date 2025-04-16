using CollegeWebsite.Models;
using MongoDB.Driver;

namespace CollegeWebsite.Services
{
    public class FacultyService : MongoDBService<Faculty>, IMongoDBService<Faculty>
    {
        public FacultyService(DatabaseSettings settings)
            : base(settings, settings.FacultyCollectionName)
        {
        }

        public async Task<Faculty?> GetByFacultyIdAsync(string facultyId)
        {
            return await FindAsync(f => f.FacultyId == facultyId).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<List<Faculty>> GetFacultyByDepartmentAsync(string departmentId)
        {
            return await FindAsync(f => f.DepartmentId == departmentId);
        }

        public async Task AssignCourseAsync(string facultyId, string courseId)
        {
            var faculty = await GetByIdAsync(facultyId);
            if (faculty != null && !faculty.CourseIds!.Contains(courseId))
            {
                faculty.CourseIds!.Add(courseId);
                await UpdateAsync(facultyId, faculty);
            }
        }

        public async Task RemoveCourseAssignmentAsync(string facultyId, string courseId)
        {
            var faculty = await GetByIdAsync(facultyId);
            if (faculty != null && faculty.CourseIds!.Contains(courseId))
            {
                faculty.CourseIds!.Remove(courseId);
                await UpdateAsync(facultyId, faculty);
            }
        }
    }
}