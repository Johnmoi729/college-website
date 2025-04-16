namespace CollegeWebsite.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string StudentsCollectionName { get; set; } = null!;
        public string CoursesCollectionName { get; set; } = null!;
        public string DepartmentsCollectionName { get; set; } = null!;
        public string FacultyCollectionName { get; set; } = null!;
        public string AdminsCollectionName { get; set; } = null!;
        public string FeedbackCollectionName { get; set; } = null!;
    }
}