using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class Student
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        [BsonElement("studentId")]
        public string StudentId { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [BsonElement("firstName")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [BsonElement("lastName")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [BsonElement("phone")]
        public string Phone { get; set; } = null!;

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("enrollmentDate")]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [BsonElement("departmentId")]
        public string? DepartmentId { get; set; }

        [BsonElement("courses")]
        public List<string>? EnrolledCourseIds { get; set; } = new List<string>();
    }
}