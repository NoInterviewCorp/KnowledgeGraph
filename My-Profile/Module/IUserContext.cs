using MongoDB.Driver;
namespace My_Profile
{
    public interface IUserContext
    {
        IMongoCollection<User> Users { get; }
    }
}