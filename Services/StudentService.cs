using CollegeWebsite.Models;
using MongoDB.Driver;

namespace CollegeWebsite.Services
{
    public class StudentService : MongoDBService<Student>, IMongoDBService<Student>
    {
        public StudentService(DatabaseSettings settings)
            : base(settings, settings.StudentsCollectionName)
        {
        }

        public async Task<Student?> GetByStudentIdAsync(string studentId)
        {
            var filter = Builders<Student>.Filter.Eq(s => s.StudentId, studentId);
            return await FindAsync(s => s.StudentId == studentId).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task<List<Student>> GetStudentsByDepartmentAsync(string departmentId)
        {
            return await FindAsync(s => s.DepartmentId == departmentId);
        }

        public async Task EnrollInCourseAsync(string studentId, string courseId)
        {
            var student = await GetByIdAsync(studentId);
            if (student != null && !student.EnrolledCourseIds!.Contains(courseId))
            {
                student.EnrolledCourseIds!.Add(courseId);
                await UpdateAsync(studentId, student);
            }
        }

        public async Task WithdrawFromCourseAsync(string studentId, string courseId)
        {
            var student = await GetByIdAsync(studentId);
            if (student != null && student.EnrolledCourseIds!.Contains(courseId))
            {
                student.EnrolledCourseIds!.Remove(courseId);
                await UpdateAsync(studentId, student);
            }
        }
    }
}