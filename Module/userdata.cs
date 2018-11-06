using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
namespace MyProfile
{
    public class UserData : IUserRepo
    {

        UserContext userprofile = null;
        public UserData(UserContext _not)
        {
            this.userprofile = _not;
        }


        public List<User> GetAllNotes()
        {
            using (userprofile)
            {

                // not.CC.Include(n=>n.topicLinks).ToList();
                 userprofile.LP.Include(n=>n.ResourceProgresses).ToList();
                return userprofile.Use.Include(n => n.learningPlans).ToList();
            }
        }
        public bool PostNote(User userprofiles)
        {
            if (userprofile.Use.FirstOrDefault(n => n.UserId == userprofiles.UserId) == null)
            {
                userprofile.Use.Add(userprofiles);
                PostLearningPlan(userprofiles);
                userprofile.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        void PostLearningPlan(User userprofiles)
        {
             if (userprofiles.learningPlans == null)
                {
                    userprofiles.learningPlans = new List<LearningPlan>();
                }
            foreach (LearningPlan cl in userprofiles.learningPlans)
            {

                userprofile.LP.Add(cl);
                PostResourceProgress(cl);
            }
            // userprofile.SaveChanges();
        }
        void PostResourceProgress(LearningPlan learning)
        {
             if (learning.ResourceProgresses == null)
                {
                    learning.ResourceProgresses = new List<ResourceProgress>();
                }
            foreach (ResourceProgress rp in learning.ResourceProgresses)
            {
             
                userprofile.RP.Add(rp);
                //  PostResourceProgress(cl);

            }
            
             userprofile.SaveChanges();
        }
        public User GetNote(string id)
        {

            using (userprofile)
            {
                // Console.WriteLine(userprofile.Use);
                userprofile.LP.Include(n => n.ResourceProgresses).ToList();
                return userprofile.Use.Include(n => n.learningPlans).ToList().FirstOrDefault(userprofiles => userprofiles.UserId == id);
            }
        }
        public List<User> GetNote(string text, string type)
        {
            List<User> Final = new List<User>();
            using (userprofile)
            {

                if (type == "username")
                {
                   userprofile.LP.Include(n => n.ResourceProgresses).ToList();
                    List<User> S_all = userprofile.Use.Where(s => s.UserName == text).Include(n => n.learningPlans).ToList();
                    return S_all;
                    //return not.Stu.Include(n=>n.labels).Include(n=>n.checkLists).ToList().FirstOrDefault(note => note.Title == text);
                }

            }
            return Final;
        }
        public void PutNote(string id, LearningPlan learningPlan)
        {
            User retrievedNote = userprofile.Use.FirstOrDefault(n => n.UserId == id);
            //LearningPlan learn=new LearningPlan();
            if (retrievedNote != null)
            {
                //  var user = not.use.Find(userParam.Id);
                if (retrievedNote.learningPlans == null)
                {
                    retrievedNote.learningPlans = new List<LearningPlan>();
                }
                
                retrievedNote.learningPlans.Add(learningPlan);
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