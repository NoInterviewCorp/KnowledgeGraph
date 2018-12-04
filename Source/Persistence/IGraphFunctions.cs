using System.Collections.Generic;
using System.Threading.Tasks;
using KnowledgeGraph.Models;
namespace KnowledgeGraph.Database.Persistence
{
    public interface IGraphFunctions
    {
        Task<LearningPlanWrapper> CreateLearningPlanAndRelationshipsAsync(LearningPlanWrapper lp);
        Task<ResourceWrapper> CreateResourceAndRelationships(ResourceWrapper resource);
        // void CreateQuestionsAndRelationships(ResourceWrapper resource);
        List<int> GetQuestionIds(string technology, string username);
        List<string> GetConceptFromTechnology(string tech);
        Dictionary<string, List<string>> GetQuestionBatchIds(string username, string technology, List<string> concepts);
        Dictionary<string, List<string>> GetQuestionIds(string username, string technology, string concept);
        Task RatingLearningPlanAndRelationshipsAsync(LearningPlanFeedBack lp);
        Task RatingResourceAndRelationshipsAsync(ResourceFeedBack Re);
        Task SubscribeLearningPlanAndRelationshipsAsync(LearningPlanFeedBack lp);
        Task UnSubscribeLearningPlanAndRelationshipsAsync(LearningPlanFeedBack lp);
        Task ReportQuestionAndRelationshipsAsync(QuestionFeedBack qe);
    }
}