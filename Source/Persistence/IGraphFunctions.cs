using System;
using System.Collections.Generic;
using KnowledgeGraph.Models;

namespace KnowledgeGraph.Persistence {
    public interface IGraphFunctions {
        void CreateLearningPlanAndRelationships (LearningPlan lp);
        void CreateResourceAndRelationships (Resource resource);
        void CreateQuestionsAndRelationships (Resource resource);
        List<int> GetQuestionBatchIds (string username, string technology, List<string> concepts);
        List<int> GetQuestionIds (string username, string technology,string concept);
    }
}