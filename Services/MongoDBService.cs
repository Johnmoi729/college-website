using CollegeWebsite.Models;
using MongoDB.Bson;  // Add this for ObjectId
using MongoDB.Driver;
using Microsoft.Extensions.Logging;  // Add this for ILogger
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CollegeWebsite.Services
{
    public class MongoDBService<T> : IMongoDBService<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;
        private readonly ILogger<MongoDBService<T>>? _logger;

        public MongoDBService(DatabaseSettings settings, string collectionName, ILogger<MongoDBService<T>>? logger = null)
        {
            try
            {
                var client = new MongoClient(settings.ConnectionString);
                var database = client.GetDatabase(settings.DatabaseName);
                _collection = database.GetCollection<T>(collectionName);
                _logger = logger;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing MongoDB connection for {Type}", typeof(T).Name);
                throw;
            }
        }

        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving all documents of type {Type}", typeof(T).Name);
                throw;
            }
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
                return await _collection.Find(filter).FirstOrDefaultAsync();
            }
            catch (FormatException ex)
            {
                _logger?.LogWarning(ex, "Invalid ObjectId format: {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving document with ID {Id}", id);
                throw;
            }
        }

        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filterExpression)
        {
            try
            {
                return await _collection.Find(filterExpression).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error finding documents with filter expression for type {Type}", typeof(T).Name);
                throw;
            }
        }

        public async Task<T> CreateAsync(T entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating document of type {Type}", typeof(T).Name);
                throw;
            }
        }

        public async Task UpdateAsync(string id, T entity)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
                await _collection.ReplaceOneAsync(filter, entity);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating document with ID {Id} of type {Type}", id, typeof(T).Name);
                throw;
            }
        }

        public async Task RemoveAsync(string id)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("_id", new ObjectId(id));
                await _collection.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing document with ID {Id} of type {Type}", id, typeof(T).Name);
                throw;
            }
        }
    }
}