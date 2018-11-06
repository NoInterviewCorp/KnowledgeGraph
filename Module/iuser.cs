using System.Collections.Generic;

namespace MyProfile{
    public interface IUserRepo{
        List<User> GetAllNotes();

        bool PostNote(User userprofiles);
       User GetNote(string Id);
       void PutNote(string id, LearningPlan userprofiles);
        List<User> GetNote(string text,string type );

    }
}