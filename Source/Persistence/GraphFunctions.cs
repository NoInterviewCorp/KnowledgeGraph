using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeGraph.Models;
using KnowledeGraph.ContentWrapper;
using KnowledgeGraph.Services;
using Neo4jClient;
using KnowledeGraph.Models;

namespace KnowledgeGraph.Database.Persistence
{
    public class GraphFunctions : IGraphFunctions
    {
        private IGraphClient graph;
        public GraphFunctions(GraphDbConnection _graphclient)
        {
            graph = _graphclient.graph;
        }
        public async Task<LearningPlanWrapper> CreateLearningPlanAndRelationshipsAsync(LearningPlanWrapper learningPlan)
        {
            try
            {
                var resources = new List<ResourceWrapper>(learningPlan.Resources);
                var technology = new TechnologyWrapper { Name = learningPlan.Technology.Name };
                var authorId = learningPlan.AuthorId;
                learningPlan.AuthorId = null;
                learningPlan.Resources = null;
                learningPlan.Technology = null;
                var queryResult = await graph.Cypher
                    .Merge("(lp:LearningPlan {LearningPlanId:\"" + learningPlan.LearningPlanId + "\"})")
                    .OnCreate()
                    .Set("lp = {learningPlan}")
                    .With("lp")
                    .Merge("(u:User{UserId:\"" + authorId + "\"})")
                    .OnCreate()
                    .Set("u={UserId:\"" + authorId + "\"}")
                    .Merge("(u)-[:DESIGNS]->(lp)")
                    .With("lp")
                    .Merge("(t:Technology {Name:\"" + technology.Name + "\"})")
                    .OnCreate()
                    .Set("t={technology}")
                    .Merge("(lp)-[:TEACHES]->(t)")
                    .WithParams(new
                    {
                        technology,
                        learningPlan
                    })
                    .Return(lp => lp.As<LearningPlanWrapper>())
                    .ResultsAsync;
                var lpResult = new List<LearningPlanWrapper>(
                    queryResult
                )[0];
                lpResult.Technology = technology;
                var resourceQuery = graph.Cypher
                    .Match(
                        "(lp:LearningPlan {LearningPlanId:\"" + learningPlan.LearningPlanId + "\"} )"
                    );
                lpResult.Resources = new List<ResourceWrapper>();
                foreach (ResourceWrapper r in resources)
                {
                    lpResult.Resources.Add(await AddResourceAsync(r));
                    resourceQuery = resourceQuery
                        .With("lp")
                        .Match(
                            "(r:Resource {ResourceId:\"" + r.ResourceId + "\"})"
                        )
                        .Merge("(lp)-[:CONTAINS]->(r)");
                }
                await resourceQuery.ExecuteWithoutResultsAsync();
                return lpResult;
            }
            catch (Exception e)
            {
                ConsoleWriter.ConsoleAnException(e);
                return null;
            }
        }

        public async Task<ResourceWrapper> CreateResourceAndRelationships(ResourceWrapper resource)
        {
            return await AddResourceAsync(resource);
        }

        public List<int> GetQuestionIds(string technology, string username)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ResourceWrapper> AddResourceAsync(ResourceWrapper resource)
        {
            // Add an unique id to resource
            // Linking technologies between resource and each of its concepts
            List<ConceptWrapper> concepts = new List<ConceptWrapper>();
            List<TechnologyWrapper> technologies = new List<TechnologyWrapper>();
            List<QuestionWrapper> questions = new List<QuestionWrapper>();

            if (resource.Concepts != null)
            {
                concepts = new List<ConceptWrapper>(resource.Concepts);
                resource.Concepts = null;
            }

            if (resource.Technologies != null)
            {
                technologies = new List<TechnologyWrapper>(resource.Technologies);
                resource.Technologies = null;
            }

            if (resource.Questions != null)
            {
                questions = new List<QuestionWrapper>(resource.Questions);
                resource.Questions = null;
            }

            // queries
            // query to create a resource
            var resQuery = new List<ResourceWrapper>(
                await graph.Cypher
                .Merge("(res:Resource {ResourceId: {resourceId} })")
                .OnCreate()
                .Set("res = {resource}")
                .WithParams(new
                {
                    resourceId = resource.ResourceId,
                    resource
                })
                .Return(res => res.As<ResourceWrapper>())
                .ResultsAsync
            )[0];
            var conceptQuery = graph.Cypher;
            var techCypherQuery = graph.Cypher;

            for (int h = 0; h < concepts.Count; h++)
            {
                ConceptWrapper concept = concepts[h];
                // Converting resource/technology/concept names to upper case
                // for faster indexed searches
                // To avoid "Property already exists" error with neo4j
                // WithParams() call
                var conceptParamsObj = new ExpandoObject() as IDictionary<string, Object>;
                conceptParamsObj.Add("conceptName_" + h, concept.Name);
                conceptParamsObj.Add("concept" + h, concept);
                conceptQuery = conceptQuery
                    .Merge($"(con{h}:Concept {{ Name: {{conceptName_{h}}}}})")
                    .OnCreate()
                    .Set($"con{h}={{concept{h}}}")
                    .With($"con{h}")
                    .Match("(r:Resource)")
                    .Where((ResourceWrapper r) => r.ResourceId == resource.ResourceId)
                    .Merge($"(r)-[:EXPLAINS]->(con{h})")
                    .WithParams(conceptParamsObj);
                for (int i = 0; i < technologies.Count; i++)
                {
                    conceptParamsObj.Add("techParamsObj" + h + i, new ExpandoObject() as IDictionary<string, object>);
                    // Converting resource/technology/concept names to upper case
                    // for faster indexed searches
                    var technology = technologies[i];
                    (conceptParamsObj["techParamsObj" + h + i] as IDictionary<string, Object>).Add("techName" + h + i, technology.Name);
                    (conceptParamsObj["techParamsObj" + h + i] as IDictionary<string, Object>).Add("technology" + h + i, technology);
                    techCypherQuery = techCypherQuery
                        .Merge($"(t{h}{i}:Technology {{ Name: {{techName{h}{i}}} }})")
                        .OnCreate()
                        .Set($"t{h}{i}={{technology{h}{i}}}")
                        .With($"t{h}{i}")
                        .Match("(con:Concept)")
                        .Where((ConceptWrapper con) => con.Name == concept.Name)
                        .Merge($"(con)-[:BELONGS_TO]->(t{h}{i})")
                        .WithParams((conceptParamsObj["techParamsObj" + h + i] as IDictionary<string, Object>));
                }
            }

            await conceptQuery.ExecuteWithoutResultsAsync();
            await techCypherQuery.ExecuteWithoutResultsAsync();

            if (questions.Count != 0)
            {
                var questionQuery = graph.Cypher;
                var questionConceptQuery = graph.Cypher;
                var questionTechQuery = graph.Cypher;
                for (int h = 0; h < questions.Count; h++)
                {
                    QuestionWrapper question = questions[h];
                    var qConcepts = new List<ConceptWrapper>(question.Concepts);
                    var qTechnology = question.Technology;
                    question.Concepts = null;
                    question.Technology = null;
                    // To avoid "Property already exists" error with neo4j
                    // WithParams() call
                    var questionParamsObj = new ExpandoObject() as IDictionary<string, Object>;
                    questionParamsObj.Add("questionId_" + h, question.QuestionId);
                    questionParamsObj.Add("question_" + h, question);
                    questionQuery = questionQuery
                        .Merge($"(q_{h}:Question {{ QuestionId: {{questionId_{h}}}}})")
                        .OnCreate()
                        .Set($"q_{h}={{question_{h}}}")
                        .With($"q_{h}")
                        .Match("(r:Resource)")
                        .Where((ResourceWrapper r) => r.ResourceId == resource.ResourceId)
                        .Merge($"(q_{h})-[:FRAMED_FROM]->(r)")
                        .WithParams(questionParamsObj);

                    if (qTechnology != null)
                    {
                        questionTechQuery = questionTechQuery
                            .Merge($"(q_{h}:Question {{ QuestionId: {{questionId_{h}}}}})")
                            .OnCreate()
                            .Set($"q_{h}={{question_{h}}}")
                            .With($"q_{h}")
                            .Merge("(t:Technology {Name:\"" + qTechnology.Name + "\"})")
                            // .Where((TechnologyWrapper t) => t.Name == qTechnology.Name)
                            .Merge($"(q_{h})-[:IS_SPECIFIC_TO]->(t)")
                            .WithParams(questionParamsObj);
                    }

                    for (int i = 0; i < qConcepts.Count; i++)
                    {
                        questionParamsObj.Add("conceptParamsObj" + h + i, new ExpandoObject() as IDictionary<string, object>);
                        var concept = qConcepts[i];
                        (questionParamsObj["conceptParamsObj" + h + i] as IDictionary<string, Object>).Add("conceptName_" + h + i, concept.Name);
                        (questionParamsObj["conceptParamsObj" + h + i] as IDictionary<string, Object>).Add("concept" + h + i, concept);
                        questionConceptQuery = questionConceptQuery
                            .Merge($"(c{h}{i}:Concept {{ Name: {{conceptName_{h}{i}}} }})")
                            .OnCreate()
                            .Set($"c{h}{i}={{concept{h}{i}}}")
                            .With($"c{h}{i}")
                            .Match("(q:Question)")
                            .Where((QuestionWrapper q) => q.QuestionId == question.QuestionId)
                            .Merge($"(q)-[:EVALUATES]->(c{h}{i})")
                            .WithParams((questionParamsObj["conceptParamsObj" + h + i] as IDictionary<string, Object>));
                    }
                    await questionQuery.ExecuteWithoutResultsAsync();
                    await questionConceptQuery.ExecuteWithoutResultsAsync();
                    await questionTechQuery.ExecuteWithoutResultsAsync();
                }
            }
            return resQuery;
        }

        public async Task<List<LearningPlanInfo>> GetLearningPlanInfoAsync(List<string> learningPlanIds)
        {
            List<LearningPlanInfo> learningPlanInfos = new List<LearningPlanInfo>();
            foreach (string learningPlanId in learningPlanIds)
            {
                LearningPlanInfo learningPlanInfo = new LearningPlanInfo() { LearningPlanId = learningPlanId };
                var avgRating = graph.Cypher
                                        .Match("(:User)-[g:RATING_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                                        .With("lp, avg(g.Rating) as avg_rating ")
                                        .Set("lp.AvgRating = avg_rating")
                                        .WithParams(new
                                        {
                                            id = learningPlanId,
                                        })
                                        .Return<float>("lp.AvgRating")
                                        .ResultsAsync;
                var totSubs = graph.Cypher
                                        .Match("(:User)-[g:Subscribe_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                                        .With("lp,  count(g.Subscribe) as total_subscriber ")
                                        .Set("lp.Subscriber = total_subscriber")
                                        .WithParams(new
                                        {
                                            id = learningPlanId,
                                        })
                                        .Return<int>("lp.Subscriber")
                                        .ResultsAsync;
                try
                {

                    learningPlanInfo.AverageRating = new List<float>
                                (
                                    await avgRating
                                )
                                .SingleOrDefault();
                }
                catch
                {
                    learningPlanInfo.AverageRating = 0;
                }
                try
                {
                    learningPlanInfo.TotalSubscribers = new List<int>
                                    (
                                        await totSubs
                                    )
                                    .SingleOrDefault();
                }
                catch
                {
                    learningPlanInfo.TotalSubscribers = 0;
                }

                learningPlanInfos.Add(learningPlanInfo);
            }
            return learningPlanInfos;
        }

        public async Task UserAndRelationshipsAsync(UserWrapper userWrapper)
        {
            await graph.Cypher
                 .Merge("(user:User { UserId: {id} })")
                 .OnCreate()
                 .Set("user = {userWrapper}")
                 .WithParams(new
                 {
                     id = userWrapper.UserId,
                     userWrapper
                 })
               .ExecuteWithoutResultsAsync();
        }

        public List<string> GetConceptFromTechnology(string tech)
        {
            List<string> data = new List<string>();
            var temp = graph.Cypher
                .Match($"(t:Technology{{ Name:'{tech}' }} )-[:COMPOSED_OF]->(c:Concept)")
                .Return(c => c.As<string>())
                .Results
                .ToList();
            data.AddRange(temp);
            return data;
        }
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
                    var shuffeledquestionsids = tempids.OrderBy(a => Guid.NewGuid()).ToList().Take(2);
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
                        var shuffeledquestionsids = tempids.OrderBy(a => Guid.NewGuid()).ToList().Take(2);
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
                        Ids.AddRange(TempIds.OrderBy(a => Guid.NewGuid()).ToList().Take(2));
                        mappedids.AddRange(Ids);
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
                })
                .Return<float>("lp.AvgRating")
                .ResultsAsync);
            Console.WriteLine("Rated LearningPlan");

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
                })
                .Return<float>("(Re.AvgRating)")
                .ResultsAsync);
            Console.WriteLine("rated resource");
        }
        public async Task SubscribeLearningPlanAndRelationshipsAsync(LearningPlanSubscriptionWrapper learningPlanSubscriptionWrapper)
        {
            await graph.Cypher
                .Match("(user:User)", "(lp:LearningPlan)")
                .Where((User user) => user.UserId == learningPlanSubscriptionWrapper.UserId)
                .AndWhere((LearningPlanWrapper lp) => lp.LearningPlanId == learningPlanSubscriptionWrapper.LearningPlanId)
                .Create("(user)-[g:Subscribe_LP]->(lp)")
                .ExecuteWithoutResultsAsync();

            var totalsubscriber = new List<int>(await graph.Cypher
               .Match("(:User)-[g:Subscribe_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                .With("lp,  count(g.Subscribe) as total_subscriber ")
                .Set("lp.Subscriber = total_subscriber")
                .WithParams(new
                {
                    id = learningPlanSubscriptionWrapper.LearningPlanId,
                })
                .Return<int>("lp.Subscriber")
                .ResultsAsync);
            Console.WriteLine("Subscribed LearningPlan");
        }
        public async Task UnSubscribeLearningPlanAndRelationshipsAsync(LearningPlanSubscriptionWrapper learningPlanSubscriptionWrapper)
        {
            await graph.Cypher
                .Match("(user:User)", "(lp:LearningPlan)")
                .Where((User user) => user.UserId == learningPlanSubscriptionWrapper.UserId)
                .AndWhere((LearningPlanWrapper lp) => lp.LearningPlanId == learningPlanSubscriptionWrapper.LearningPlanId)

                .Merge("(user)-[g:Subscribe_LP]->(lp)")
                .Delete("g")
                .ExecuteWithoutResultsAsync();
            var totalsubscriber = new List<int>(await graph.Cypher
                .Match("(:User)-[g:Subscribe_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                .With("lp,  count(g.Subscribe) as total_subscriber ")
                .Set("lp.Subscriber = total_subscriber")
                .WithParams(new
                {
                    id = learningPlanSubscriptionWrapper.LearningPlanId,
                })
                .Return<int>("lp.Subscriber")
                .ResultsAsync);
            Console.WriteLine("Unsubscribed LearningPlan");
        }
        public async Task ReportQuestionAndRelationshipsAsync(QuestionAmbiguityWrapper questionAmbiguityWrapper)
        {
            await graph.Cypher
                .Match("(user:User)", "(qe:Question)")
                .Where((User user) => user.UserId == questionAmbiguityWrapper.UserId)
                .AndWhere((QuestionWrapper qe) => qe.QuestionId == questionAmbiguityWrapper.QuestionId)

                .Merge("(user)-[g:Report_Question]->(qe)")
                .OnCreate()
                .Set("g={QuestionReport}")
                .OnMatch()
                .Set("g={QuestionReport}")
                .ExecuteWithoutResultsAsync();
            var totalReport = new List<int>(await graph.Cypher
               .Match("(:User)-[g:Report_Question]->(qe:Question {QuestionId:{id}})")
                .With("qe,  count(g.ambigous) as total_ambiguity ")
                .Set("qe.Total_Ambiguity = total_ambiguity")
                .WithParams(new
                {
                    id = questionAmbiguityWrapper.QuestionId,
                })
                .Return<int>("qe.Total_Ambiguity")
                .ResultsAsync);
            Console.WriteLine("Question Reported");
            Console.WriteLine("ques is ambigius");
        }
        public void IncreaseIntensityOnConcept(string username, string concept, int bloom)
        {
            switch (bloom)
            {
                case 1:
                    graph.Cypher
                        .Match($"(u:User{{ UserId: '{username}' }})-[R:Knowledge]-(c:Concept{{ Name:'{concept}'}})")
                        .Set("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults();
                    break;
                case 2:
                    graph.Cypher
                        .Match($"(u:User{{ UserId: '{username}' }})-[R:Comprehension]-(c:Concept{{ Name:'{concept}'}})")
                        .Set("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults();
                    break;
                case 3:
                    graph.Cypher
                        .Match($"(u:User{{ UserId: '{username}' }})-[R:Application]-(c:Concept{{ Name:'{concept}'}})")
                        .Set("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults();
                    break;
                case 4:
                    graph.Cypher
                        .Match($"(u:User{{ UserId: '{username}' }})-[R:Analysis]-(c:Concept{{ Name:'{concept}'}})")
                        .Set("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults();
                    break;
                case 5:
                    graph.Cypher
                        .Match($"(u:User{{ UserId: '{username}' }})-[R:Synthesis]-(c:Concept{{ Name:'{concept}'}})")
                        .Set("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults();
                    break;
                case 6:
                    graph.Cypher
                        .Match($"(u:User{{ UserId: '{username}' }})-[R:Evaluation]-(c:Concept{{ Name:'{concept}'}})")
                        .Set("R.Intensity = R.Intensity+1")
                        .ExecuteWithoutResults();
                    break;
            }
            Console.WriteLine("---Intensity increased in {0}---", (BloomTaxonomy)bloom);
        }

        public async Task<UserReport> GenerateUserReport(string userId)
        {
            var testedTechs = new List<Technology>(
                    await graph.Cypher
                        .Match($"(u:User{{UserId:'{userId}' }})-[:EVALUATED_HIMSELF_ON]->(t:Technology)")
                        .Return(t => t.As<Technology>())
                        .ResultsAsync
                );
            var userReport = new UserReport() { UserId = userId };
            foreach (var tech in testedTechs)
            {
                var testedConcepts = new List<Concept>(
                    await graph.Cypher
                        .Match($"(u:User {{UserId:'{userId}' }} )-[:TESTED_HIMSELF_ON]->(c:Concept)-[:BELONGS_TO]->(t:Technology{{Name:'{tech.Name}'}})")
                        .Return(c => c.As<Concept>())
                        .ResultsAsync
                );
                tech.Concepts = testedConcepts;
                var technologyReport = new TechnologyReport() { TechnologyName = tech.Name };
                foreach (var concept in testedConcepts)
                {
                    var conceptReportIEnum = graph.Cypher
                        .Match($"(u:User{{UserId: '{userId}' }} )")
                        .With("u")
                        .Match(
                            $"(u)-[k:Knowledge]->(:Concept {{Name: '{concept.Name}' }}",
                            $"(u)-[co:Comprehension]->(:Concept {{Name: '{concept.Name}' }}",
                            $"(u)-[ap:Application]->(:Concept {{Name: '{concept.Name}' }}",
                            $"(u)-[an:Analysis]->(:Concept {{Name: '{concept.Name}' }}",
                            $"(u)-[s:Synthesis]->(:Concept {{Name: '{concept.Name}' }}",
                            $"(u)-[e:Evaluation]->(:Concept {{Name: '{concept.Name}' }}"
                        )
                        .With("k.Intensity as kI")
                        .With("co.Intensity as coI")
                        .With("ap.Intensity as apI")
                        .With("an.Intensiy as anI")
                        .With("s.Intensity as sI")
                        .With("e.Intensity as eI")
                        .Return<ConceptReport>((kI, coI, apI, anI, sI, eI) => new ConceptReport
                        {
                            KnowledgeIntensity = kI.As<int>(),
                            ComprehensionIntensity = coI.As<int>(),
                            ApplicationIntensity = apI.As<int>(),
                            AnalysisIntensity = anI.As<int>(),
                            SynthesisIntensity = sI.As<int>(),
                            EvaluationIntensity = eI.As<int>()
                        })
                        .ResultsAsync;
                    var conceptReport = new List<ConceptReport>(await conceptReportIEnum).SingleOrDefault();
                    if (conceptReport == null)
                    {
                        conceptReport = new ConceptReport();
                    }
                    conceptReport.ConceptName = concept.Name;
                    technologyReport.ConceptReports.Add(conceptReport);
                }
                userReport.TechnologyReports.Add(technologyReport);
            }
            return userReport;
        }
    }
}
