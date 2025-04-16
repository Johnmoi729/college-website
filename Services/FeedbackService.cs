using CollegeWebsite.Models;
using MongoDB.Driver;

namespace CollegeWebsite.Services
{
    public class FeedbackService : MongoDBService<Feedback>, IMongoDBService<Feedback>
    {
        public FeedbackService(DatabaseSettings settings)
            : base(settings, settings.FeedbackCollectionName)
        {
        }

        // Additional feedback-specific methods can be added here if needed
        // For example, methods to get unresolved feedback, mark feedback as resolved, etc.
        public async Task<List<Feedback>> GetUnresolvedFeedbackAsync()
        {
            return await FindAsync(f => !f.IsResolved);
        }

        public async Task MarkAsResolvedAsync(string id)
        {
            var feedback = await GetByIdAsync(id);
            if (feedback != null)
            {
                feedback.IsResolved = true;
                await UpdateAsync(id, feedback);
            }
    }
}
}