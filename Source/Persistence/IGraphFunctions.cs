using System.Collections.Generic;
using System.Threading.Tasks;
using KnowledeGraph.ContentWrapper;
using KnowledeGraph.Models;
using KnowledgeGraph.Models;
namespace KnowledgeGraph.Database.Persistence
{
    public interface IGraphFunctions
    {
        Task<LearningPlanWrapper> CreateLearningPlanAndRelationshipsAsync(LearningPlanWrapper lp);
        Task<ResourceWrapper> CreateResourceAndRelationships(ResourceWrapper resource);
        Task<List<LearningPlanInfo>> GetLearningPlanInfoAsync(List<string> learningPlanIds);
        Task<UserReport> GenerateUserReport(string userId);
        List<string> GetQuestionBatchIds(string username, string technology, List<string> concepts);
        Task RatingLearningPlanAndRelationshipsAsync(LearningPlanRatingWrapper lpr);
        Task RatingResourceAndRelationshipsAsync(ResourceRatingWrapper Re);
        Task SubscribeLearningPlanAndRelationshipsAsync(LearningPlanSubscriptionWrapper lp);
         Task SubscribeLearningPlanAndRelationshipsAsync1(string id);
        Task UnSubscribeLearningPlanAndRelationshipsAsync(LearningPlanSubscriptionWrapper lp);
        Task ReportQuestionAndRelationshipsAsync(QuestionAmbiguityWrapper qe);
        Task UserAndRelationshipsAsync(UserWrapper userWrapper);
        void IncreaseIntensityOnConcept(string username, string concept, int bloom);
        Task PopularLearningPlanAndRelationshipsAsync1(string techName);
    }
}