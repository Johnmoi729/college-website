using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Reflection;

namespace CollegeWebsite.Utilities
{
    public static class MongoSchemaValidator
    {
        public static async Task ValidateCollectionSchema<T>(IMongoCollection<T> collection, ILogger logger = null) where T : class
        {
            try
            {
                // Get a sample document
                var document = await collection.Find(_ => true).Limit(1).FirstOrDefaultAsync();
                if (document == null)
                {
                    logger?.LogInformation("No documents found in collection for {Type}", typeof(T).Name);
                    return;
                }

                // Convert to BsonDocument
                var bsonDoc = document.ToBsonDocument();

                // Get all properties with BsonElement attributes
                var propertyMappings = typeof(T).GetProperties()
                    .Select(p => new
                    {
                        Property = p.Name,
                        BsonField = p.GetCustomAttribute<BsonElementAttribute>()?.ElementName ?? p.Name
                    })
                    .ToDictionary(x => x.BsonField, x => x.Property);

                // Check fields in document against properties
                foreach (var field in bsonDoc.Names)
                {
                    if (field == "_id") continue; // Skip _id field

                    if (!propertyMappings.ContainsKey(field))
                    {
                        logger?.LogWarning("Field '{Field}' in MongoDB does not map to any property in {Type}", field, typeof(T).Name);
                    }
                }

                logger?.LogInformation("Schema validation completed for {Type}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error validating schema for {Type}", typeof(T).Name);
            }
        }
    }
}