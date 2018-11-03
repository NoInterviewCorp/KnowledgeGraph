using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
namespace MyProfile {
    public class UserData : IUserRepo{
        
        UserContext not=null;
        public UserData( UserContext _not)
        {
            this.not=_not;
        }
        
        
        public List<User> GetAllNotes(){
            using(not)
            {
                
               // not.CC.Include(n=>n.topicLinks).ToList();
               // not.LP.Include(n=>n.CourseContents).ToList();
                return not.Use.ToList();
            }
        }
         public bool PostNote(User note){
            if(not.Use.FirstOrDefault(n => n.UserId == note.UserId) == null){
                not.Use.Add(note);
                not.SaveChanges();
                return true;
            }
            else{
                return false;
            }
        }
      

     
        

       

        
       
    }
}