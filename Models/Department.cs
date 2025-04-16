using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class Department
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Department code is required")]
        [BsonElement("departmentCode")]
        public string DepartmentCode { get; set; } = null!;

        [Required(ErrorMessage = "Department name is required")]
        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("headOfDepartment")]
        public string? HeadOfDepartmentId { get; set; }

        [BsonElement("faculty")]
        public List<string>? FacultyIds { get; set; } = new List<string>();

        [BsonElement("courses")]
        public List<string>? CourseIds { get; set; } = new List<string>();
    }
}