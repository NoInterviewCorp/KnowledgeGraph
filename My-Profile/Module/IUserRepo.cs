using System.Collections.Generic;
using System.Threading.Tasks;
namespace My_Profile
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllGames();
        Task<User> GetUser(string name);
        Task Create(User user);
        Task<bool> Update(User user);
        Task<bool> Delete(string name);
    }
}