using System;
using System.Collections.Generic;
using System.Linq;
using KnowledgeGraph.Models;
using KnowledgeGraph.Services;
namespace KnowledgeGraph.Persistence {
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

        public Dictionary<string, List<string>> GetQuestionBatchIds (string username, string technology, List<string> concepts) {
            List<string> Ids = new List<string> ();
            Dictionary<string, List<string>> mappedids = new Dictionary<string, List<string>> ();
            var data = graphclient.graph.Cypher
                .Match ("(u:User{username:{user}})-[:HAS_ACCUMULATED_KNOWLEDGE_ON ]-(tech:Technology{{tech}})-[:ON]-(c:Concept)")
                .WithParams (new {
                    user = username,
                        tech = technology
                })
                .Return (c => c.As<Concept> ())
                .Results
                .ToList ();
            foreach (var tempdata in data) {
                int flag = 0;
                foreach (var comingconcept in concepts) {
                    if (tempdata.Name == comingconcept) {
                        flag = 1;
                    }
                }
                switch (flag) {
                    case 1:
                        var tempids = graphclient.graph.Cypher
                            .Match ("(q:Question)-[:EVALUATES]-(c:Concept{name:{name}})")
                            .WithParams (new {
                                name = tempdata.Name
                            })
                            .Where ("q.bloom<={bloom}")
                            .WithParams (new {
                                bloom = (int) tempdata.bloomtaxonomy
                            })
                            .Return (q => q.As<Question> ().Id)
                            .Results
                            .ToList ();
                        Ids.AddRange (tempids);
                        mappedids.Add (tempdata.Name, Ids);
                        break;
                    default:
                        var tempid = graphclient.graph.Cypher
                            .Match ("(q:Question{bloom:1)-[:EVALUATES]-(c:Concept{name:{name}})")
                            .WithParams (new {
                                name = tempdata.Name
                            })
                            .Return (q => q.As<Question> ().Id)
                            .Results
                            .ToList ();
                        Ids.AddRange (tempid);
                        mappedids.Add (tempdata.Name, Ids);
                        break;
                }
            }
            return mappedids;
        }

        public Dictionary<string, List<string>> GetQuestionIds (string username, string technology, string concept) {
           Dictionary<string, List<string>> Ids = new Dictionary<string, List<string>> ();
            var tempid = graphclient.graph.Cypher
                .Match ("(q:Question{bloom:1)-[:EVALUATES]-(c:Concept{name:{name}})")
                .WithParams (new {
                    name = concept
                })
                .Return (q => q.As<Question> ().Id)
                .Results
                .ToList ();
            Ids.Add(concept,tempid);
            return Ids;
        }
    }
}