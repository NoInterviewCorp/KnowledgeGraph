using System.Collections.Generic;
using KnowledgeGraph.Database.Models;

namespace KnowledgeGraph.Database.Persistence
{
    public interface IGraphFunctions
    {
        void CreateLearningPlanAndRelationships(LearningPlan lp);
        void CreateResourceAndRelationships(Resource resource);
        void CreateQuestionsAndRelationships(Resource resource);
        List<int> GetQuestionIds(string technology,string username);
    }
}