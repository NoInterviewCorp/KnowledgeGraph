using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeGraph.Models;
using KnowledeGraph.ContentWrapper;
using KnowledgeGraph.Services;
using Neo4jClient;

namespace KnowledgeGraph.Database.Persistence {
    public class GraphFunctions : IGraphFunctions {
        private IGraphClient graph;
        public GraphFunctions (GraphDbConnection _graphclient) {
            graph = _graphclient.graph;
        }
        public async Task<LearningPlanWrapper> CreateLearningPlanAndRelationshipsAsync (LearningPlanWrapper learningPlan) {
            try {
                var resources = new List<ResourceWrapper> (learningPlan.Resources);
                var technology = new TechnologyWrapper { Name = learningPlan.Technology.Name };
                var authorId = learningPlan.AuthorId;
                learningPlan.AuthorId = null;
                learningPlan.Resources = null;
                learningPlan.Technology = null;
                var queryResult = await graph.Cypher
                    .Merge ("(lp:LearningPlan {LearningPlanId:\"" + learningPlan.LearningPlanId + "\"})")
                    .OnCreate ()
                    .Set ("lp = {learningPlan}")
                    .With ("lp")
                    .Match ("(u:User{UserId:\"" + authorId + "\"})")
                    .Merge ("(u)-[:DESIGNS]->(lp)")
                    .With ("lp")
                    .Merge ("(t:Technology {Name:\"" + technology.Name + "\"})")
                    .OnCreate ()
                    .Set ("t={technology}")
                    .Merge ("(lp)-[:TEACHES]->(t)")
                    .WithParams (new {
                        technology,
                        learningPlan
                    })
                    .Return (lp => lp.As<LearningPlanWrapper> ())
                    .ResultsAsync;
                var lpResult = new List<LearningPlanWrapper> (
                    queryResult
                ) [0];
                lpResult.Technology = technology;
                var resourceQuery = graph.Cypher
                    .Match (
                        "(lp:LearningPlan {LearningPlanId:\"" + learningPlan.LearningPlanId + "\"} )"
                    );
                lpResult.Resources = new List<ResourceWrapper> ();
                foreach (ResourceWrapper r in resources) {
                    lpResult.Resources.Add (await AddResourceAsync (r));
                    resourceQuery = resourceQuery
                        .With ("lp")
                        .Match (
                            "(r:Resource {ResourceId:\"" + r.ResourceId + "\"})"
                        )
                        .Merge ("(lp)-[:CONTAINS]->(r)");
                }
                Console.WriteLine ("----------------LP->RESOURCE----------------------------");
                Console.WriteLine (resourceQuery.Query.QueryText);
                await resourceQuery.ExecuteWithoutResultsAsync ();
                return lpResult;
            } catch (Exception e) {
                Console.WriteLine ("----------------------EXCEPTION-MESSAGE------------------------------------");
                Console.WriteLine (e.Message);
                Console.WriteLine ("----------------------STACK-TRACE-----------------------------------------");
                Console.WriteLine (e.StackTrace);
                Console.WriteLine ("-------------------------INNER-EXCEPTION-----------------------------");
                Console.WriteLine (e.InnerException);
                return null;
            }
            // return null;
        }

        // public void CreateQuestionsAndRelationships(ResourceWrapper resource)
        // {
        //     throw new System.NotImplementedException();
        // }

        public async Task<ResourceWrapper> CreateResourceAndRelationships (ResourceWrapper resource) {
            return await AddResourceAsync (resource);
        }

        public List<int> GetQuestionIds (string technology, string username) {
            throw new System.NotImplementedException ();
        }

        public async Task<ResourceWrapper> AddResourceAsync (ResourceWrapper resource) {
            // Add an unique id to resource
            // Linking technologies between resource and each of its concepts
            List<ConceptWrapper> concepts = new List<ConceptWrapper> ();
            List<TechnologyWrapper> technologies = new List<TechnologyWrapper> ();
            List<QuestionWrapper> questions = new List<QuestionWrapper> ();

            if (resource.Concepts != null) {
                concepts = new List<ConceptWrapper> (resource.Concepts);
                resource.Concepts = null;
            }

            if (resource.Technologies != null) {
                technologies = new List<TechnologyWrapper> (resource.Technologies);
                resource.Technologies = null;
            }

            if (resource.Questions != null) {
                questions = new List<QuestionWrapper> (resource.Questions);
                resource.Questions = null;
            }

            // queries
            // query to create a resource
            var resQuery = new List<ResourceWrapper> (
                await graph.Cypher
                .Merge ("(res:Resource {ResourceId: {resourceId} })")
                .OnCreate ()
                .Set ("res = {resource}")
                .WithParams (new {
                    resourceId = resource.ResourceId,
                        resource
                })
                .Return (res => res.As<ResourceWrapper> ())
                .ResultsAsync
            ) [0];
            var conceptQuery = graph.Cypher;
            var techCypherQuery = graph.Cypher;

            for (int h = 0; h < concepts.Count; h++) {
                ConceptWrapper concept = concepts[h];
                // Converting resource/technology/concept names to upper case
                // for faster indexed searches
                // To avoid "Property already exists" error with neo4j
                // WithParams() call
                var conceptParamsObj = new ExpandoObject () as IDictionary<string, Object>;
                conceptParamsObj.Add ("conceptName_" + h, concept.Name);
                conceptParamsObj.Add ("concept" + h, concept);
                conceptQuery = conceptQuery
                    .Merge ($"(con{h}:Concept {{ Name: {{conceptName_{h}}}}})")
                    .OnCreate ()
                    .Set ($"con{h}={{concept{h}}}")
                    .With ($"con{h}")
                    .Match ("(r:Resource)")
                    .Where ((ResourceWrapper r) => r.ResourceId == resource.ResourceId)
                    .Merge ($"(r)-[:EXPLAINS]->(con{h})")
                    .WithParams (conceptParamsObj);
                for (int i = 0; i < technologies.Count; i++) {
                    conceptParamsObj.Add ("techParamsObj" + h + i, new ExpandoObject () as IDictionary<string, object>);
                    // Converting resource/technology/concept names to upper case
                    // for faster indexed searches
                    var technology = technologies[i];
                    (conceptParamsObj["techParamsObj" + h + i] as IDictionary<string, Object>).Add ("techName" + h + i, technology.Name);
                    (conceptParamsObj["techParamsObj" + h + i] as IDictionary<string, Object>).Add ("technology" + h + i, technology);
                    techCypherQuery = techCypherQuery
                        .Merge ($"(t{h}{i}:Technology {{ Name: {{techName{h}{i}}} }})")
                        .OnCreate ()
                        .Set ($"t{h}{i}={{technology{h}{i}}}")
                        .With ($"t{h}{i}")
                        .Match ("(con:Concept)")
                        .Where ((ConceptWrapper con) => con.Name == concept.Name)
                        .Merge ($"(con)-[:BELONGS_TO]->(t{h}{i})")
                        .WithParams ((conceptParamsObj["techParamsObj" + h + i] as IDictionary<string, Object>));
                }
            }

            await conceptQuery.ExecuteWithoutResultsAsync ();
            await techCypherQuery.ExecuteWithoutResultsAsync ();

            if (questions.Count != 0) {
                var questionQuery = graph.Cypher;
                var questionConceptQuery = graph.Cypher;
                var questionTechQuery = graph.Cypher;
                for (int h = 0; h < questions.Count; h++) {
                    QuestionWrapper question = questions[h];
                    var qConcepts = new List<ConceptWrapper> (question.Concepts);
                    var qTechnology = question.Technology;
                    question.Concepts = null;
                    question.Technology = null;
                    // To avoid "Property already exists" error with neo4j
                    // WithParams() call
                    var questionParamsObj = new ExpandoObject () as IDictionary<string, Object>;
                    questionParamsObj.Add ("questionId_" + h, question.QuestionId);
                    questionParamsObj.Add ("question_" + h, question);
                    questionQuery = questionQuery
                        .Merge ($"(q_{h}:Question {{ QuestionId: {{questionId_{h}}}}})")
                        .OnCreate ()
                        .Set ($"q_{h}={{question_{h}}}")
                        .With ($"q_{h}")
                        .Match ("(r:Resource)")
                        .Where ((ResourceWrapper r) => r.ResourceId == resource.ResourceId)
                        .Merge ($"(q_{h})-[:FRAMED_FROM]->(r)")
                        .WithParams (questionParamsObj);

                    if (qTechnology != null) {
                        questionTechQuery = questionTechQuery
                            .Merge ($"(q_{h}:Question {{ QuestionId: {{questionId_{h}}}}})")
                            .OnCreate ()
                            .Set ($"q_{h}={{question_{h}}}")
                            .With ($"q_{h}")
                            .Merge ("(t:Technology {Name:\"" + qTechnology.Name + "\"})")
                            // .Where((TechnologyWrapper t) => t.Name == qTechnology.Name)
                            .Merge ($"(q_{h})-[:IS_SPECIFIC_TO]->(t)")
                            .WithParams (questionParamsObj);
                    }

                    for (int i = 0; i < qConcepts.Count; i++) {
                        questionParamsObj.Add ("conceptParamsObj" + h + i, new ExpandoObject () as IDictionary<string, object>);
                        var concept = qConcepts[i];
                        (questionParamsObj["conceptParamsObj" + h + i] as IDictionary<string, Object>).Add ("conceptName_" + h + i, concept.Name);
                        (questionParamsObj["conceptParamsObj" + h + i] as IDictionary<string, Object>).Add ("concept" + h + i, concept);
                        questionConceptQuery = questionConceptQuery
                            .Merge ($"(c{h}{i}:Concept {{ Name: {{conceptName_{h}{i}}} }})")
                            .OnCreate ()
                            .Set ($"c{h}{i}={{concept{h}{i}}}")
                            .With ($"c{h}{i}")
                            .Match ("(q:Question)")
                            .Where ((QuestionWrapper q) => q.QuestionId == question.QuestionId)
                            .Merge ($"(q)-[:EVALUATES]->(c{h}{i})")
                            .WithParams ((questionParamsObj["conceptParamsObj" + h + i] as IDictionary<string, Object>));
                    }
                    Console.WriteLine ("---------------------QUESTION-CREATE-----------------------------");
                    Console.WriteLine (questionQuery.Query.QueryText);
                    await questionQuery.ExecuteWithoutResultsAsync ();
                    Console.WriteLine ("---------------------QUESTION->CONCEPT------------------------------------");
                    Console.WriteLine (questionConceptQuery.Query.QueryText);
                    await questionConceptQuery.ExecuteWithoutResultsAsync ();
                    Console.WriteLine ("---------------------QUESTION->TECHNOLOGY---------------------------------");
                    Console.WriteLine (questionTechQuery.Query.QueryText);
                    await questionTechQuery.ExecuteWithoutResultsAsync ();
                }
            }
            return resQuery;
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
            var tempid = graph.Cypher
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
            var temp = graph.Cypher
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
            var quizgivencounter = graph.Cypher
                .Match ($"path = (u:User{{ UserName:{username}}})-[:EVALUATED_HIMSELF_ON]-(t:Technology{{ Name:{technology} }})")
                .Return<int> ("count(path)")
                .Results
                .ToList ();
            if (quizgivencounter[0] != 1) {
                graph.Cypher
                    .Match ($"path = (u:User{{ UserName:{username}}})")
                    .Match ($"(t:Technology{{ Name:{technology} }})")
                    .Create ("(u)-[:EVALUATED_HIMSELF_ON]-(t)")
                    .ExecuteWithoutResults ();
                foreach (var concept in concepts) {
                    graph.Cypher
                        .Match ($"(u:User{{ UserName:{username} }})")
                        .Match ($"(c:Concept{{ Name:{concept} }})")
                        .Create ("(u)-[:TESTED_HIMSELF_ON]-(c)")
                        .Create ("(u)-[:KNOWLEDGE{Intensity:0}]-(c)")
                        .Create ("(u)-[:COMPREHENSION{Intensity:0}]-(c)")
                        .Create ("(u)-[:APPLICATION{Intensity:0}]-(c)")
                        .Create ("(u)-[:ANALYSIS{Intensity:0}]-(c)")
                        .Create ("(u)-[:SYTHENSIS{Intensity:0}]-(c)")
                        .Create ("(u)-[:EVALUATION{Intensity:0}]-(c)")
                        .ExecuteWithoutResults ();
                    var tempids = graph.Cypher
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
                    var conceptattemptedearlier = graph.Cypher
                        .Match ($"path = (u:User{{ UserName:{username} }})-[eval:TESTED_HIMSELF_ON]-(c:Concept{{ Name:{concept} }})")
                        .Return<int> ("count(path)")
                        .Results
                        .ToList ();
                    if (conceptattemptedearlier[0] < 1) {
                        graph.Cypher
                            .Match ($"(u:User{{ UserName:{username} }})")
                            .Match ($"(c:Concept{{ Name:{concept} }})")
                            .Create ("(u)-[:TESTED_HIMSELF_ON]-(c)")
                            .Create ("(u)-[:KNOWLEDGE{Intensity:0}]-(c)")
                            .Create ("(u)-[:COMPREHENSION{Intensity:0}]-(c)")
                            .Create ("(u)-[:APPLICATION{Intensity:0}]-(c)")
                            .Create ("(u)-[:ANALYSIS{Intensity:0}]-(c)")
                            .Create ("(u)-[:SYTHENSIS{Intensity:0}]-(c)")
                            .Create ("(u)-[:EVALUATION{Intensity:0}]-(c)")
                            .ExecuteWithoutResults ();
                        var tempids = graph.Cypher
                            .Match ($"(q:Question)-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                            .Return (q => q.As<Question> ().Id)
                            .Results
                            .ToList ();
                        var shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                        Ids.Clear ();
                        Ids.AddRange (shuffeledquestionsids);
                        mappedids.Add (concept, Ids);
                    } else {
                        var relations = graph.Cypher
                            .Match ($"path = (u:User{{ UserName:{username} }})-[R]-(c:Concept{{ Name:{concept} }})")
                            .Where ("not type(R)=\"TESTED_HIMSELF_ON\"")
                            .OrderBy ("R.Intensity")
                            .ReturnDistinct<string> ("type(R)")
                            .Results
                            .ToList ().Take (3);
                        List<string> TempIds = new List<string> ();
                        foreach (var relation in relations) {
                            switch (relation) {
                                case "KNOWLEDGE":
                                    var tempids = graph.Cypher
                                        .Match ($"(q:Question{{Bloom:1}})-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                                        .Return (q => q.As<Question> ().Id)
                                        .Results
                                        .ToList ();
                                    var shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                                    // Ids.Clear ();
                                    TempIds.AddRange (shuffeledquestionsids);
                                    // mappedids.Add (concept, Ids);
                                    break;
                                case "COMPREHENSION":
                                    tempids = graph.Cypher
                                        .Match ($"(q:Question{{Bloom:1}})-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                                        .Return (q => q.As<Question> ().Id)
                                        .Results
                                        .ToList ();
                                    shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                                    // Ids.Clear ();
                                    TempIds.AddRange (shuffeledquestionsids);
                                    // mappedids.Add (concept, Ids);
                                    break;
                                case "APPLICATION":
                                    tempids = graph.Cypher
                                        .Match ($"(q:Question{{Bloom:1}})-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                                        .Return (q => q.As<Question> ().Id)
                                        .Results
                                        .ToList ();
                                    shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                                    // Ids.Clear ();
                                    TempIds.AddRange (shuffeledquestionsids);
                                    // mappedids.Add (concept, Ids);
                                    break;
                                case "ANALYSIS":
                                    tempids = graph.Cypher
                                        .Match ($"(q:Question{{Bloom:1}})-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                                        .Return (q => q.As<Question> ().Id)
                                        .Results
                                        .ToList ();
                                    shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                                    // Ids.Clear ();
                                    TempIds.AddRange (shuffeledquestionsids);
                                    // mappedids.Add (concept, Ids);
                                    break;
                                case "SYTHENSIS":
                                    tempids = graph.Cypher
                                        .Match ($"(q:Question{{Bloom:1}})-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                                        .Return (q => q.As<Question> ().Id)
                                        .Results
                                        .ToList ();
                                    shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                                    // Ids.Clear ();
                                    TempIds.AddRange (shuffeledquestionsids);
                                    // mappedids.Add (concept, Ids);
                                    break;
                                case "EVALUATION":
                                    tempids = graph.Cypher
                                        .Match ($"(q:Question{{Bloom:1}})-[:EVALUATES]-(c:Concept{{ name:{concept} }})")
                                        .Return (q => q.As<Question> ().Id)
                                        .Results
                                        .ToList ();
                                    shuffeledquestionsids = tempids.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10);
                                    // Ids.Clear ();
                                    TempIds.AddRange (shuffeledquestionsids);
                                    // mappedids.Add (concept, Ids);
                                    break;
                            }
                        }
                        Ids.Clear ();
                        Ids.AddRange (TempIds.OrderBy (a => Guid.NewGuid ()).ToList ().Take (10));
                        mappedids.Add (concept, Ids);
                    }
                }
            }
            return mappedids;
        }
        public async Task RatingLearningPlanAndRelationshipsAsync(LearningPlanRatingWrapper learningPlanRatingWrapper)
        {
           
                GiveStarPayload giveStar = new GiveStarPayload { Rating = learningPlanRatingWrapper.Star };
                await graph.Cypher
                    .Match("(user:User)", "(lp:LearningPlan)")
                    .Where((User user) => user.UserId == learningPlanRatingWrapper.UserId)
                    .AndWhere((LearningPlanWrapper lp) => lp.LearningPlanId == learningPlanRatingWrapper.LearningPlanId)
                    .Merge("(user)-[g:RATING_LP]->(lp)")
                    .OnCreate()
                    .Set("g={giveStar}")
                    .OnMatch()
                    .Set("g={giveStar}")
                    .WithParams(new
                    {
                        userRating = learningPlanRatingWrapper.Star,
                        giveStar
                    })
                    .ExecuteWithoutResultsAsync();
                var LPqueryAvg = new List<float>(await graph.Cypher
                    .Match("(:User)-[g:RATING_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                    .With("lp,  avg(g.Rating) as avg_rating ")
                    .Set("lp.AvgRating = avg_rating")
                    .WithParams(new
                    {
                        id = learningPlanRatingWrapper.LearningPlanId,
                        // rating=
                    })
                    .Return<float>("lp.AvgRating")
                    .ResultsAsync);
              //  var avg_rating = LPqueryAvg[0];
                Console.WriteLine("Rated LearningPlan");
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                //  return  Ok(new List<float>(LPqueryAvg)[0]);
            
            // return Ok(new List<float>(LPqueryAvg));
            // throw new NotImplementedException();
        }
        public async Task RatingResourceAndRelationshipsAsync(ResourceRatingWrapper resourceRatingWrapper)
        {
        GiveStarPayload giveStar = new GiveStarPayload { Rating = resourceRatingWrapper.Star };
            await graph.Cypher
                .Match("(user:User)", "(Re:Resource)")
                .Where((User user) => user.UserId == resourceRatingWrapper.UserId)
                .AndWhere((ResourceWrapper Re) => Re.ResourceId == resourceRatingWrapper.ResourceId)
                .Merge("(user)-[g:RATING_Resource]->(Re)")
                .OnCreate()
                .Set("g={giveStar}")
                .OnMatch()
                .Set("g={giveStar}")
                .WithParams(new
                {
                    userRating = resourceRatingWrapper.Star,
                    giveStar
                })
                .ExecuteWithoutResultsAsync();
            var Re_queryAvg = new List<float>(await graph.Cypher
                .Match("(:User)-[g:RATING_Resource]->(Re:Resource {ResourceId:{id}})")
                .With("Re,  avg(g.Rating) as avg_rating ")
                .Set("Re.AvgRating = avg_rating")
                .WithParams(new
                {
                    id = resourceRatingWrapper.ResourceId,
                    // rating=
                })
                .Return<float> ("(Re.AvgRating)")
                .ResultsAsync);
            Console.WriteLine ("rated resource");
            // return Ok(new List<float>(Re_queryAvg)[0]);
        }
        public async Task SubscribeLearningPlanAndRelationshipsAsync(LearningPlanSubscriptionWrapper learningPlanSubscriptionWrapper)
        {
           // GiveStarPayload LearningPlanSubscriber = new GiveStarPayload { Subscribe = learningPlanFeedback.Subscribe };
            await graph.Cypher
                .Match("(user:User)", "(lp:LearningPlan)")
                .Where((User user) => user.UserId == learningPlanSubscriptionWrapper.UserId)
                .AndWhere((LearningPlanWrapper lp) => lp.LearningPlanId == learningPlanSubscriptionWrapper.LearningPlanId)

                .Create("(user)-[g:Subscribe_LP]->(lp)")
                //.OnCreate()
                //.Set("g={LearningPlanSubscriber}")
                //.OnMatch()
               // .Set("g={LearningPlanSubscriber}")
               // .WithParams(new
               // {
                   // usersubscribe = learningPlanFeedback.Subscribe,
                   // LearningPlanSubscriber
              //  })
                .ExecuteWithoutResultsAsync();
            var totalsubscriber = new List<int>(await graph.Cypher
               .Match("(:User)-[g:Subscribe_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                // .Match((GiveStarPayload sub)=>sub.Subscribe==1)
                .With("lp,  count(g.Subscribe) as total_subscriber ")
                .Set("lp.Subscriber = total_subscriber")
                .WithParams(new
                {
                    id = learningPlanSubscriptionWrapper.LearningPlanId,
                    // rating=
                })
                .Return<int> ("lp.Subscriber")

                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                .ResultsAsync);
                Console.WriteLine("Subscribed LearningPlan");
         //   return Ok(new List<int>(totalsubscriber)[0]);
        }
         public async Task UnSubscribeLearningPlanAndRelationshipsAsync(LearningPlanSubscriptionWrapper learningPlanSubscriptionWrapper)
        {
            //GiveStarPayload LearningPlanSubscriber = new GiveStarPayload { Subscribe = learningPlanFeedback.Subscribe };
            await graph.Cypher
                .Match("(user:User)", "(lp:LearningPlan)")
                .Where((User user) => user.UserId == learningPlanSubscriptionWrapper.UserId)
                .AndWhere((LearningPlanWrapper lp) => lp.LearningPlanId == learningPlanSubscriptionWrapper.LearningPlanId)

                .Merge ("(user)-[g:Subscribe_LP]->(lp)")
                .Delete ("g")
                .ExecuteWithoutResultsAsync ();
            var totalsubscriber = new List<int> (await graph.Cypher
                .Match ("(:User)-[g:Subscribe_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                // .Match((GiveStarPayload sub)=>sub.Subscribe==1)
                .With("lp,  count(g.Subscribe) as total_subscriber ")
                .Set("lp.Subscriber = total_subscriber")
                .WithParams(new
                {
                    id = learningPlanSubscriptionWrapper.LearningPlanId,
                    // rating=
                })
                .Return<int> ("lp.Subscriber")
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                .ResultsAsync);
                Console.WriteLine("Unsubscribed LearningPlan");
            //return Ok(new List<int>(totalsubscriber)[0]);
        }
        public async Task ReportQuestionAndRelationshipsAsync(QuestionAmbiguityWrapper questionAmbiguityWrapper)
        {
        // GiveStarPayload QuestionReport = new GiveStarPayload { Ambigous = questionFeedBack.Ambiguity };
            await graph.Cypher
                .Match("(user:User)", "(qe:Question)")
                .Where((User user) => user.UserId == questionAmbiguityWrapper.UserId)
                .AndWhere((QuestionWrapper qe) => qe.QuestionId == questionAmbiguityWrapper.QuestionId)

                .Merge("(user)-[g:Report_Question]->(qe)")
                .OnCreate()
                .Set("g={QuestionReport}")
                .OnMatch()
                .Set("g={QuestionReport}")
                //.WithParams(new
               // {
               //     userreport = questionFeedBack.Ambiguity,
               //     QuestionReport
               // })
                .ExecuteWithoutResultsAsync();
            var totalReport = new List<int>(await graph.Cypher
               .Match("(:User)-[g:Report_Question]->(qe:Question {QuestionId:{id}})")
                // .Match((GiveStarPayload sub)=>sub.Subscribe==1)
                .With("qe,  count(g.ambigous) as total_ambiguity ")
                .Set("qe.Total_Ambiguity = total_ambiguity")
                .WithParams(new
                {
                    id = questionAmbiguityWrapper.QuestionId,
                    // rating=
                })
                .Return<int> ("qe.Total_Ambiguity")
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                .ResultsAsync);
                Console.WriteLine("Question Reported");
           // return Ok(new List<int>(totalReport)[0]);
            Console.WriteLine ("ques is ambigius");
            // return Ok(new List<int>(totalReport)[0]);
        }
        public void IncreaseIntensityOnConcept (string username, string concept, int bloom) {
            switch (bloom) {
                case 1:
                    graph.Cypher
                        .Match ($"(u:User{{ UserName:{username}}})-[R:KNOWLEDGE]-(c:Concept{{ Name:{concept}}})")
                        .Set ("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults ();
                    break;
                case 2:
                    graph.Cypher
                        .Match ($"(u:User{{ UserName:{username}}})-[R:COMPREHENSION]-(c:Concept{{ Name:{concept}}})")
                        .Set ("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults ();
                    break;
                case 3:
                    graph.Cypher
                        .Match ($"(u:User{{ UserName:{username}}})-[R:APPLICATION]-(c:Concept{{ Name:{concept}}})")
                        .Set ("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults ();
                    break;
                case 4:
                    graph.Cypher
                        .Match ($"(u:User{{ UserName:{username}}})-[R:ANALYSIS]-(c:Concept{{ Name:{concept}}})")
                        .Set ("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults ();
                    break;
                case 5:
                    graph.Cypher
                        .Match ($"(u:User{{ UserName:{username}}})-[R:SYNTHESIS]-(c:Concept{{ Name:{concept}}})")
                        .Set ("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults ();
                    break;
                case 6:
                    graph.Cypher
                        .Match ($"(u:User{{ UserName:{username}}})-[R:EVALUATION]-(c:Concept{{ Name:{concept}}})")
                        .Set ("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults ();
                    break;
            }
        }
    }
}
