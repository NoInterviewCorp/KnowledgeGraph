using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace My_Profile
{
    public class UserRepository : IUserRepository
    {
        private readonly IUserContext _context;
        public UserRepository(IUserContext context)
        {
            _context = context;
        }
        public async Task<List<User>> GetAllUsers()
        {
            return await _context
                            .Users
                            .Find(_ => true)
                            .ToListAsync();
        }
        public Task<User> GetUser(ObjectId id)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(m => m.Id, id);
            return _context
                    .Users
                    .Find(filter)
                    .FirstOrDefaultAsync();
        }

        public async Task Create(User user)
        {
            
            await _context.Users.InsertOneAsync(user);
        }
         public async Task<bool> PostNote(User user)
        {
           //  FilterDefinition<User> filter = Builders<User>.Filter.Eq(m => m.Id==user.Id);
           bool exists = await _context.Users.Find((n => n.UserName == user.UserName)).AnyAsync();
            if(exists){
                return true;
            }
            else{
                return false;
            }
           
        }
         public async Task<bool> FindNote(MongoDB.Bson.ObjectId id)
        {
           //  FilterDefinition<User> filter = Builders<User>.Filter.Eq(m => m.Id==user.Id);
           bool exists = await _context.Users.Find((n => n.Id == id)).AnyAsync();
            if(exists){
                return true;
            }
            else{
                return false;
            }
           
        }
        public async Task<bool> Update(User user)
        {
            ReplaceOneResult updateResult =
                await _context
                        .Users
                        .ReplaceOneAsync(
                            filter: g => g.Id == user.Id,
                            replacement: user);
            return updateResult.IsAcknowledged
                    && updateResult.ModifiedCount > 0;
        }
        public async Task<bool> Delete(MongoDB.Bson.ObjectId id)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(m => m.Id, id);
            DeleteResult deleteResult = await _context
                                                .Users
                                                .DeleteOneAsync(filter);
            return deleteResult.IsAcknowledged
                && deleteResult.DeletedCount > 0;
        }
    }
}