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
                 return not.Use.Include(n=>n.learningPlans).ToList();
            }
        }
          public bool PostNote(User note){
            if(not.Use.FirstOrDefault(n => n.UserId == note.UserId) == null){
                not.Use.Add(note);
                PostChecklist(note);
                not.SaveChanges();
                return true;
            }
            else{
                return false;
            }
        }
        void PostChecklist(User note){
            foreach(LearningPlan cl in note.learningPlans){
                not.LP.Add(cl);
            }
            not.SaveChanges();
        }
         public LearningPlan GetNote(string id){
            using(not)
            {
            return not.LP.ToList().FirstOrDefault(note => note.LearningPlanId == id);
            }
        }
        public List<User> GetNote(string text,string type){
            List<User> Final = new List<User>();
            using(not)
            {

                if(type=="username")    
                {
                    List<User> S_all = not.Use.Where(s=> s.UserName==text).Include(n=>n.learningPlans).ToList();
                    return S_all;
            //return not.Stu.Include(n=>n.labels).Include(n=>n.checkLists).ToList().FirstOrDefault(note => note.Title == text);
                }
               
            }
            return Final;
        }

     
        

       

        
       
    }
}