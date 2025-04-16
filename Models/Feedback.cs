using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class Feedback
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Subject is required")]
        [BsonElement("subject")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Message is required")]
        [BsonElement("message")]
        public string Message { get; set; } = null!;

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        [BsonElement("rating")]
        public int Rating { get; set; } = 5;

        [BsonElement("submissionDate")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [BsonElement("isResolved")]
        public bool IsResolved { get; set; } = false;
    }
}