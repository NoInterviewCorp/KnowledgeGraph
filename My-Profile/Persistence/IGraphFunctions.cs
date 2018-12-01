using System.Collections.Generic;
using System.Threading.Tasks;

namespace My_Profile.Persistence
{
    public interface IGraphFunctions
    {
        Task RatingLearningPlanAndRelationshipsAsync(LearningPlanFeedBack lp);
        Task RatingResourceAndRelationshipsAsync(ResourceFeedBack Re);
        Task SubscribeLearningPlanAndRelationshipsAsync(LearningPlanFeedBack lp);
        Task UnSubscribeLearningPlanAndRelationshipsAsync(LearningPlanFeedBack lp);
        Task ReportQuestionAndRelationshipsAsync(QuestionFeedBack qe);
      //  Task<ResourceWrapper> CreateResourceAndRelationships(ResourceWrapper resource);
        // void CreateQuestionsAndRelationships(ResourceWrapper resource);
       // List<int> GetQuestionIds(string technology,string username);
    }
}