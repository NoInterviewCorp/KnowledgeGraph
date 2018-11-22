using System.Collections.Generic;
using KnowledgeGraph.Database.Models;
using KnowledgeGraph.Services;

namespace KnowledgeGraph.Database.Persistence {
    public class GraphFunctions : IGraphFunctions {
        private GraphDbConnection graphclient;
        public GraphFunctions (GraphDbConnection _graphclient) {
            graphclient = _graphclient;
        }
        public void CreateLearningPlanAndRelationships (LearningPlan lp) {
            throw new System.NotImplementedException ();
        }

        public void CreateQuestionsAndRelationships (Resource resource) {
            throw new System.NotImplementedException ();
        }

        public void CreateResourceAndRelationships (Resource resource) {
            throw new System.NotImplementedException ();
        }

        public List<int> GetQuestionIds (string technology, string username) {
            throw new System.NotImplementedException ();
        }
    }
}