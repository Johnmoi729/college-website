using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    [BsonIgnoreExtraElements]
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

        // This was likely "headOfDepartment" in database but you had "headOfDepartmentId" in code
        [BsonElement("headOfDepartmentId")]
        public string? HeadOfDepartmentId { get; set; }

        // This was "faculty" in database but you had "FacultyIds" in code without proper mapping
        [BsonElement("faculty")]
        public List<string>? FacultyIds { get; set; } = new List<string>();

        // This was "courses" in database but you had "CourseIds" in code without proper mapping
        [BsonElement("courses")]
        public List<string>? CourseIds { get; set; } = new List<string>();
    }
}