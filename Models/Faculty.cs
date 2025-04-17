using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    [BsonIgnoreExtraElements]
    public class Faculty
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Faculty ID is required")]
        [BsonElement("facultyId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FacultyId { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [BsonElement("firstName")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required")]
        [BsonElement("lastName")]
        public string LastName { get; set; } = null!;

        // Computed property for full name
        public string Name => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [BsonElement("phone")]
        public string Phone { get; set; } = null!;

        [BsonElement("position")]
        public string? Position { get; set; }

        [BsonElement("office")]
        public string? Office { get; set; }

        [BsonElement("departmentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? DepartmentId { get; set; }

        [BsonElement("courses")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string>? CourseIds { get; set; } = new List<string>();

        [BsonElement("joinDate")]
        public DateTime JoinDate { get; set; } = DateTime.Now;
    }
}