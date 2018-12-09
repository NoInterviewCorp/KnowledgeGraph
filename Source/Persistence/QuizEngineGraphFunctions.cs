using System;
using System.Collections.Generic;
using System.Linq;
using KnowledgeGraph.Models;
using KnowledgeGraph.RabbitMQModels;
using KnowledgeGraph.Services;
using Neo4jClient;

namespace KnowledgeGraph.Database.Persistence
{
    public partial class GraphFunctions
    {
        public List<string> GetQuestionBatchIds(string username, string technology, List<string> concepts)
        {
            List<string> Ids = new List<string>();
            List<string> mappedids = new List<string>();
            Console.WriteLine("Checking whether {0} has given quiz on this technology or not", username);
            var quizgivencounter = graph.Cypher
                .Match($"path = (u:User{{ UserId: '{username}' }})-[:EVALUATED_HIMSELF_ON]-(t:Technology{{ Name:'{technology}' }})")
                .ReturnDistinct<int>("count(path)")
                .OrderByDescending("count(path)")
                .Results
                .SingleOrDefault();
            Console.WriteLine(quizgivencounter);
            if (quizgivencounter < 1)
            {
                graph.Cypher
                    .Match($"(u:User{{ UserId:'{username}' }})")
                    .Match($"(t:Technology{{ Name:'{technology}' }})")
                    .Create("(u)-[:EVALUATED_HIMSELF_ON]->(t)")
                    .ExecuteWithoutResults();
                foreach (var concept in concepts)
                {
                    graph.Cypher
                        .Match($"(u:User{{ UserId: '{username}' }})")
                        .Match($"(c:Concept{{ Name: '{concept}' }})")
                        .Merge("(u)-[:TESTED_HIMSELF_ON]->(c)")
                        .Merge("(u)-[K:Knowledge]->(c)")
                        .OnCreate().Set("K.Intensity = 0")
                        .Merge("(u)-[C:Comprehension]->(c)")
                        .OnCreate().Set("C.Intensity = 0")
                        .Merge("(u)-[A:Application]->(c)")
                        .OnCreate().Set("A.Intensity = 0")
                        .Merge("(u)-[An:Analysis]->(c)")
                        .OnCreate().Set("An.Intensity = 0")
                        .Merge("(u)-[S:Synthesis]->(c)")
                        .OnCreate().Set("S.Intensity = 0")
                        .Merge("(u)-[E:Evaluation]->(c)")
                        .OnCreate().Set("E.Intensity = 0")
                        .ExecuteWithoutResults();
                    var tempids = graph.Cypher
                        .Match($"(q:Question)-[:EVALUATES]-(c:Concept{{ Name: '{concept}' }})")
                        .Return(q => q.As<Question>().QuestionId)
                        .Results
                        .ToList();
                    var shuffeledquestionsids = tempids.OrderBy(a => Guid.NewGuid()).ToList().Take(10);
                    Ids.Clear();
                    Ids.AddRange(shuffeledquestionsids);
                    mappedids.AddRange(Ids);
                }
            }
            else
            {
                foreach (var concept in concepts)
                {
                    var conceptattemptedearlier = graph.Cypher
                        .Match($"path = (u:User{{ UserId: '{username}' }})-[:TESTED_HIMSELF_ON]-(c:Concept{{ Name: '{concept}' }})")
                        .Return<int>("count(path)")
                        .OrderByDescending("count(path)")
                        .Results
                        .SingleOrDefault();
                    Console.WriteLine(conceptattemptedearlier);
                    if (conceptattemptedearlier < 1)
                    {
                        graph.Cypher
                            .Match($"(u:User{{ UserId: '{username}' }})")
                            .Match($"(c:Concept{{ Name: '{concept}' }})")
                            .Merge("(u)-[:TESTED_HIMSELF_ON]->(c)")
                            .Merge("(u)-[K:Knowledge]->(c)")
                            .OnCreate().Set("K.Intensity = 0")
                            .Merge("(u)-[C:Comprehension]->(c)")
                            .OnCreate().Set("C.Intensity = 0")
                            .Merge("(u)-[A:Application]->(c)")
                            .OnCreate().Set("A.Intensity = 0")
                            .Merge("(u)-[An:Analysis]->(c)")
                            .OnCreate().Set("An.Intensity = 0")
                            .Merge("(u)-[S:Synthesis]->(c)")
                            .OnCreate().Set("S.Intensity = 0")
                            .Merge("(u)-[E:Evaluation]->(c)")
                            .OnCreate().Set("E.Intensity = 0")
                            .ExecuteWithoutResults();
                        var tempids = graph.Cypher
                            .Match($"(q:Question)-[:EVALUATES]->(c:Concept{{ Name: '{concept}' }})")
                            .Return(q => q.As<Question>().QuestionId)
                            .Results
                            .ToList();
                        var shuffeledquestionsids = tempids.OrderBy(a => Guid.NewGuid()).ToList().Take(10);
                        Ids.Clear();
                        Ids.AddRange(shuffeledquestionsids);
                        mappedids.AddRange(Ids);
                    }
                    else
                    {
                        var relations = graph.Cypher
                            .Match($"path = (u:User{{ UserId: '{username}' }})-[R]-(c:Concept{{ Name: '{concept}' }})")
                            .Where("not type(R)=\"TESTED_HIMSELF_ON\"")
                            .Return<string>("type(R)")
                            .OrderBy("R.Intensity")
                            .Results
                            .ToList().Take(3);
                        List<string> TempIds = new List<string>();
                        foreach (var relation in relations)
                        {
                            var tempids = graph.Cypher
                                        .Match($"(q:Question{{BloomLevel: '{relation}' }})-[:EVALUATES]-(c:Concept{{ Name:'{concept}' }})")
                                        .Return(q => q.As<Question>().QuestionId)
                                        .Results
                                        .ToList();
                            var shuffeledquestionsids = tempids.OrderBy(a => Guid.NewGuid()).ToList().Take(2);
                            TempIds.AddRange(shuffeledquestionsids);
                        }
                        Ids.Clear();
                        Ids.AddRange(TempIds.OrderBy(a => Guid.NewGuid()).ToList().Take(10));
                        mappedids.AddRange(Ids);
                    }
                }
            }
            return mappedids;
        }
        public void IncreaseIntensityOnConcept(string username, string concept, int bloom)
        {
            var intensity = (BloomTaxonomy)bloom;
            graph.Cypher
                .Match($"(u:User{{ UserId: '{username}' }})-[R:{intensity}]-(c:Concept{{ Name:'{concept}'}})")
                .Set("R.Intensity = R.Intensity+1")
                .ExecuteWithoutResults();
            Console.WriteLine("---Intensity increased in {0}---", intensity);
        }
        public List<string> RecommendResource(string username)
        {
            List<string> ResourceIds = new List<string>();
            var intensity = graph.Cypher
                .Match($"(u:User{{UserId: '{username}'-[R]-(c:Concept) }})")
                .With("sum(R.Intensity) as sumI, c.Name as cName")
                // .Return<IntensityMap>("sum(R.Intensity),c.Name")
                .Return<IntensityMap>((sumI,cName)=>new IntensityMap
                {
                    Intensity = sumI.As<int>(),
                    Name = cName.As<string>()
                })
                .OrderByDescending("sum(sumI)")
                .Results
                .ToList().Take(3);
            foreach (var concept in intensity)
            {
                var Ids = graph.Cypher
                    .Match($"(r:Resource)-[:EXPLAINS]-(c:Concept{{Name:'{concept.Name}' }})")
                    .Return<string>("r.ResourceId")
                    .Results
                    .ToList().Take(5);
                ResourceIds.AddRange(Ids);
                
            }
            return ResourceIds;
        }
    }
}