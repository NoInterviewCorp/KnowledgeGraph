using System.Collections.Generic;
using System.Threading.Tasks;
using KnowledgeGraph.Database.Models;

namespace KnowledgeGraph.Database.Persistence
{
    public interface IGraphFunctions
    {
        Task<LearningPlanWrapper> CreateLearningPlanAndRelationshipsAsync(LearningPlanWrapper lp);
        Task<ResourceWrapper> CreateResourceAndRelationships(ResourceWrapper resource);
        // void CreateQuestionsAndRelationships(ResourceWrapper resource);
        List<int> GetQuestionIds(string technology,string username);
    }
}