using System;
using System.Collections.Generic;
using KnowledgeGraph.Models;

namespace KnowledgeGraph.Persistence {
    public interface IGraphFunctions {
        void CreateLearningPlanAndRelationships (LearningPlan lp);
        void CreateResourceAndRelationships (Resource resource);
        void CreateQuestionsAndRelationships (Resource resource);
        List<string> GetConceptFromTechnology (string tech);
        Dictionary<string, List<string>> GetQuestionBatchIds (string username, string technology, List<string> concepts);
        Dictionary<string, List<string>> GetQuestionIds (string username, string technology, string concept);
    }
}