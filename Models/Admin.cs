using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class Admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [BsonElement("username")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [BsonElement("password")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("fullName")]
        public string? FullName { get; set; }

        [BsonElement("role")]
        public string Role { get; set; } = "Admin";

        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}