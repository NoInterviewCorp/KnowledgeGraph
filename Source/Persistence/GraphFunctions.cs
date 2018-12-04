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

        // public Dictionary<string, List<string>> GetQuestionBatchIds (string username, string technology, List<string> concepts) {
        //     List<string> Ids = new List<string> ();
        //     Dictionary<string, List<string>> mappedids = new Dictionary<string, List<string>> ();
        //     var data = graphclient.graph.Cypher
        //         .Match ("(u:User{username:{user}})-[:HAS_ACCUMULATED_KNOWLEDGE_ON ]-(tech:Technology{{tech}})-[:ON]-(c:Concept)")
        //         .WithParams (new {
        //             user = username,
        //                 tech = technology
        //         })
        //         .Return (c => c.As<Concept> ())
        //         .Results
        //         .ToList ();
        //     foreach (var tempdata in data) {
        //         int flag = 0;
        //         foreach (var comingconcept in concepts) {
        //             if (tempdata.Name == comingconcept) {
        //                 flag = 1;
        //             }
        //         }
        //         switch (flag) {
        //             case 1:
        //                 var tempids = graphclient.graph.Cypher
        //                     .Match ("(q:Question)-[:EVALUATES]-(c:Concept{name:{name}})")
        //                     .WithParams (new {
        //                         name = tempdata.Name
        //                     })
        //                     .Where ("q.bloom<={bloom}")
        //                     .WithParams (new {
        //                         bloom = (int) tempdata.bloomtaxonomy
        //                     })
        //                     .Return (q => q.As<Question> ().Id)
        //                     .Results
        //                     .ToList ();
        //                 Ids.AddRange (tempids);
        //                 mappedids.Add (tempdata.Name, Ids);
        //                 break;
        //             default:
        //                 var tempid = graphclient.graph.Cypher
        //                     .Match ("(q:Question{bloom:1)-[:EVALUATES]-(c:Concept{name:{name}})")
        //                     .WithParams (new {
        //                         name = tempdata.Name
        //                     })
        //                    .Return (q => q.As<Question> ().Id)
        //                    .Results
        //                    .ToList ();
        //                 Ids.AddRange (tempid);
        //                 mappedids.Add (tempdata.Name, Ids);
        //                 break;
        //         }
        //     }
        //     return mappedids;
        // }

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
            Ids.Add (concept, tempid);
            return Ids;
        }
        public List<string> GetConceptFromTechnology (string tech) {
            List<string> data = new List<string> ();
            var temp = graphclient.graph.Cypher
                .Match ($"(t:Technology{{ Name:{tech} }} )-[:COMPOSED_OF]->(c:Concept)")
                .Return (c => c.As<string> ())
                .Results
                .ToList ();
            data.AddRange (temp);
            return data;
        }

        public Dictionary<string, List<string>> GetQuestionBatchIds (string username, string technology, List<string> concepts) {
            List<string> Ids = new List<string> ();
            Dictionary<string, List<string>> mappedids = new Dictionary<string, List<string>> ();
            var quizgivencounter = graphclient.graph.Cypher
                .Match ($"path = (u:User{{ UserName:{username}}})-[:EVALUATED_HIMSELF_ON]-(t:Technology{{ Name:{technology} }})")
                .Return<int> ("count(path)")
                .Results
                .ToList ();
            if (quizgivencounter[0] != 1) {
                graphclient.graph.Cypher
                    .Match ($"path = (u:User{{ UserName:{username}}})")
                    .Match ($"(t:Technology{{ Name:{technology} }})")
                    .Create ("(u)-[:EVALUATED_HIMSELF_ON]-(t)")
                    .ExecuteWithoutResults ();
                foreach (var concept in concepts) {
                    graphclient.graph.Cypher
                        .Match ($"(u:User{{ UserName:{username} }})")
                        .Match ($"(c:Concept{{ Name:{concept} }})")
                        .Create ("(u)-[:TESTED_HIMSELF_ON]-(c)")
                        .Create ("(u)-[:BLOOM_TEXANOMY_1{Intensity:0}]-(c)")
                        .Create ("(u)-[:BLOOM_TEXANOMY_2{Intensity:0}]-(c)")
                        .Create ("(u)-[:BLOOM_TEXANOMY_3{Intensity:0}]-(c)")
                        .Create ("(u)-[:BLOOM_TEXANOMY_4{Intensity:0}]-(c)")
                        .Create ("(u)-[:BLOOM_TEXANOMY_5{Intensity:0}]-(c)")
                        .Create ("(u)-[:BLOOM_TEXANOMY_6{Intensity:0}]-(c)")
                        .ExecuteWithoutResults ();
                    var tempids = graphclient.graph.Cypher
                        .Match ($"(q:Question)-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                        .Return (q => q.As<Question> ().Id)
                        .Results
                        .ToList ();
                    var shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                    Ids.Clear ();
                    Ids.AddRange (shuffeledquestionsids);
                    mappedids.Add (concept, Ids);
                }
            } else {
                foreach (var concept in concepts) {
                    var conceptattemptedearlier = graphclient.graph.Cypher
                        .Match ($"path = (u:User{{ UserName:{username} }})-[eval:TESTED_HIMSELF_ON]-(c:Concept{{ Name:{concept} }})")
                        .Return<int> ("count(path)")
                        .Results
                        .ToList ();
                    if (conceptattemptedearlier[0] < 1) {
                        graphclient.graph.Cypher
                            .Match ($"(u:User{{ UserName:{username} }})")
                            .Match ($"(c:Concept{{ Name:{concept} }})")
                            .Create ("(u)-[:TESTED_HIMSELF_ON]-(c)")
                            .Create ("(u)-[:BLOOM_TEXANOMY_1{Intensity:0}]-(c)")
                            .Create ("(u)-[:BLOOM_TEXANOMY_2{Intensity:0}]-(c)")
                            .Create ("(u)-[:BLOOM_TEXANOMY_3{Intensity:0}]-(c)")
                            .Create ("(u)-[:BLOOM_TEXANOMY_4{Intensity:0}]-(c)")
                            .Create ("(u)-[:BLOOM_TEXANOMY_5{Intensity:0}]-(c)")
                            .Create ("(u)-[:BLOOM_TEXANOMY_6{Intensity:0}]-(c)")
                            .ExecuteWithoutResults ();
                        var tempids = graphclient.graph.Cypher
                            .Match ($"(q:Question)-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                            .Return (q => q.As<Question> ().Id)
                            .Results
                            .ToList ();
                        var shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                        Ids.Clear ();
                        Ids.AddRange (shuffeledquestionsids);
                        mappedids.Add (concept, Ids);
                    } else {
                        var relations = graphclient.graph.Cypher
                            .Match ($"path = (u:User{{ UserName:{username} }})-[R]-(c:Concept{{ Name:{concept} }})")
                            .OrderBy("count(path)")
                            .ReturnDistinct<string>("type(R)")
                            .Results
                            .ToList().Take(3);
                        foreach (var relation in relations)
                        {
                            
                        }
                    }
                }
            }
            return mappedids;
        }
    }
}