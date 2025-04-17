using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    [BsonIgnoreExtraElements]
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Course code is required")]
        [BsonElement("courseCode")]
        public string CourseCode { get; set; } = null!;

        [Required(ErrorMessage = "Course name is required")]
        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Credits are required")]
        [Range(1, 6, ErrorMessage = "Credits must be between 1 and 6")]
        [BsonElement("credits")]
        public int Credits { get; set; }

        [BsonElement("departmentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? DepartmentId { get; set; }

        [BsonElement("facultyId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? FacultyId { get; set; }

        // Alias for FacultyId to support existing views
        public string? InstructorId => FacultyId;

        [BsonElement("schedule")]
        public string? Schedule { get; set; }

        [BsonElement("capacity")]
        public int Capacity { get; set; } = 30;

        [BsonElement("enrolledStudents")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string>? EnrolledStudentIds { get; set; } = new List<string>();
    }
}