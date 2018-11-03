using System.Collections.Generic;

namespace MyProfile{
    public interface IUserRepo{
        List<User> GetAllNotes();

        bool PostNote(User note);

       
    }
}