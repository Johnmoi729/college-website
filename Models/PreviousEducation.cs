using MongoDB.Bson.Serialization.Attributes;

namespace CollegeWebsite.Models
{
    public class PreviousEducation
    {
        [BsonElement("university")]
        public string? University { get; set; }

        [BsonElement("enrollmentNumber")]
        public string? EnrollmentNumber { get; set; }

        [BsonElement("center")]
        public string? Center { get; set; }

        [BsonElement("stream")]
        public string? Stream { get; set; }

        [BsonElement("field")]
        public string? Field { get; set; }

        [BsonElement("marksSecured")]
        public float MarksSecured { get; set; }

        [BsonElement("outOf")]
        public float OutOf { get; set; }

        [BsonElement("classObtained")]
        public string? ClassObtained { get; set; }
    }
}