using System.Collections.Generic;

namespace MyProfile{
    public interface IUserRepo{
        List<User> GetAllNotes();

        bool PostNote(User note);
         LearningPlan GetNote(string Id);
        List<User> GetNote(string text,string type );

    }
}