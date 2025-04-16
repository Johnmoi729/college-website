using CollegeWebsite.Models;
using MongoDB.Driver;

namespace CollegeWebsite.Services
{
    public class CourseService : MongoDBService<Course>, IMongoDBService<Course>
    {
        public CourseService(DatabaseSettings settings)
            : base(settings, settings.CoursesCollectionName)
        {
        }

        public async Task<Course?> GetByCourseCodeAsync(string courseCode)
        {
            return await FindAsync(c => c.CourseCode == courseCode).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<List<Course>> GetCoursesByDepartmentAsync(string departmentId)
        {
            return await FindAsync(c => c.DepartmentId == departmentId);
        }

        public async Task<List<Course>> GetCoursesByFacultyAsync(string facultyId)
        {
            return await FindAsync(c => c.FacultyId == facultyId);
        }

        public async Task EnrollStudentAsync(string courseId, string studentId)
        {
            var course = await GetByIdAsync(courseId);
            if (course != null && !course.EnrolledStudentIds!.Contains(studentId) && course.EnrolledStudentIds!.Count < course.Capacity)
            {
                course.EnrolledStudentIds!.Add(studentId);
                await UpdateAsync(courseId, course);
            }
        }

        public async Task RemoveStudentAsync(string courseId, string studentId)
        {
            var course = await GetByIdAsync(courseId);
            if (course != null && course.EnrolledStudentIds!.Contains(studentId))
            {
                course.EnrolledStudentIds!.Remove(studentId);
                await UpdateAsync(courseId, course);
            }
        }
    }
}