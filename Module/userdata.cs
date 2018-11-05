using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
namespace MyProfile {
    public class UserData : IUserRepo{
        
        UserContext userprofile=null;
        public UserData( UserContext _not)
        {
            this.userprofile=_not;
        }
        
        
        public List<User> GetAllNotes(){
            using(userprofile)
            {
                
               // not.CC.Include(n=>n.topicLinks).ToList();
               // not.LP.Include(n=>n.CourseContents).ToList();
                 return userprofile.Use.Include(n=>n.learningPlans).ToList();
            }
        }
          public bool PostNote(User userprofiles){
            if(userprofile.Use.FirstOrDefault(n => n.UserId == userprofiles.UserId) == null){
                userprofile.Use.Add(userprofiles);
                PostChecklist(userprofiles);
               userprofile.SaveChanges();
                return true;
            }
            else{
                return false;
            }
        }
        void PostChecklist(User userprofiles){
            foreach(LearningPlan cl in userprofiles.learningPlans){
                userprofile.LP.Add(cl);
            }
            userprofile.SaveChanges();
        }
         public LearningPlan GetNote(string id){
            using(userprofile)
            {
            return userprofile.LP.ToList().FirstOrDefault(userprofiles => userprofiles.LearningPlanId == id);
            }
        }
        public List<User> GetNote(string text,string type){
            List<User> Final = new List<User>();
            using(userprofile)
            {

                if(type=="username")    
                {
                    List<User> S_all = userprofile.Use.Where(s=> s.UserName==text).Include(n=>n.learningPlans).ToList();
                    return S_all;
            //return not.Stu.Include(n=>n.labels).Include(n=>n.checkLists).ToList().FirstOrDefault(note => note.Title == text);
                }
               
            }
            return Final;
        }
         public void PutNote(string id, LearningPlan userprofiles){
             User retrievedNote = userprofile.Use.FirstOrDefault(n => n.UserId == id);
             //LearningPlan learn=new LearningPlan();
             if( retrievedNote != null){
          //  var user = not.use.Find(userParam.Id);
                if(retrievedNote.learningPlans==null){
                    retrievedNote.learningPlans = new List<LearningPlan>();
                }
               retrievedNote.learningPlans.Add(userprofiles);
                // not.Stu.Add(note);
                userprofile.SaveChanges();
               // return true;
             }
            // }
            // else{
            //     return false;
            // }
        }

     
        

       

        
       
    }
}