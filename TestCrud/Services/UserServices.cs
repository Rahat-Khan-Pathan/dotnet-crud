using CrudApp.Controllers;
using CrudApp.DB;
using CrudApp.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TestCrud.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestCrud.Services
{
    public class UserServices
    {
        private readonly IMongoCollection<User> _userCollection;
        public UserServices(IMongoCollection<User> userCollection)
        {
            _userCollection = userCollection;
        }
        public async Task<ApiResponse<User>> CreateUserServiceAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, salt);
            await _userCollection.InsertOneAsync(user);
            user.Password = "";
            return new ApiResponse<User>
            {
                Message = "User created successfully",
                Data = user
            };
        }
        public async Task<User> GetUserByIdAsync(string id)
        {
            var projection = Builders<User>.Projection.Exclude(u => u.Password);
            var filter = Builders<User>.Filter.Eq("_id", new ObjectId(id));
            var customer = await _userCollection.Find(filter).Project<User>(projection).FirstOrDefaultAsync();
            return customer;
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq("email", email.ToString());
            var customer = await _userCollection.Find(filter).FirstOrDefaultAsync();
            return customer;
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            var projection = Builders<User>.Projection.Exclude(u => u.Password);
            return await _userCollection.Find(FilterDefinition<User>.Empty).Project<User>(projection).ToListAsync();
        }
    }
}
