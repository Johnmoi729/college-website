using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    [BsonIgnoreExtraElements]
    public class Student
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Student ID is required")]
        [BsonElement("studentId")]
        public string StudentId { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [BsonElement("firstName")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [BsonElement("lastName")]
        public string LastName { get; set; } = null!;

        [BsonElement("fatherName")]
        public string? FatherName { get; set; }

        [BsonElement("motherName")]
        public string? MotherName { get; set; }

        [BsonElement("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [BsonElement("gender")]
        public string? Gender { get; set; }

        [BsonElement("residentialAddress")]
        public string? ResidentialAddress { get; set; }

        [BsonElement("permanentAddress")]
        public string? PermanentAddress { get; set; }

        [BsonElement("previousEducation")]
        public List<PreviousEducation>? PreviousEducation { get; set; }

        [BsonElement("sportsDetails")]
        public string? SportsDetails { get; set; }

        [BsonElement("admissionStatus")]
        public string AdmissionStatus { get; set; } = "Pending"; // Options: Pending, Accepted, Rejected

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        [BsonElement("phone")]
        public string Phone { get; set; } = null!;

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("enrollmentDate")]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [BsonElement("departmentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? DepartmentId { get; set; }

        [BsonElement("enrolledCourseIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string>? EnrolledCourseIds { get; set; } = new List<string>();
    }
}