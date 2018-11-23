using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace My_Profile
{
    public class UserRepository : IUserRepository
    {
        private readonly IUserContext _context;
        public UserRepository(IUserContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<User>> GetAllGames()
        {
            return await _context
                            .Users
                            .Find(_ => true)
                            .ToListAsync();
        }
        public Task<User> GetUser(string name)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(m => m.UserName, name);
            return _context
                    .Users
                    .Find(filter)
                    .FirstOrDefaultAsync();
        }

        public async Task Create(User user)
        {
            await _context.Users.InsertOneAsync(user);
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
        public async Task<bool> Delete(string name)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(m => m.UserName, name);
            DeleteResult deleteResult = await _context
                                                .Users
                                                .DeleteOneAsync(filter);
            return deleteResult.IsAcknowledged
                && deleteResult.DeletedCount > 0;
        }
    }
}