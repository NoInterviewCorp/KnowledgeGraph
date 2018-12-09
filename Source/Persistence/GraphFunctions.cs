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
    public partial class GraphFunctions : IGraphFunctions
    {
        private IGraphClient graph;
        public GraphFunctions(GraphDbConnection _graphclient)
        {
            graph = _graphclient.graph;
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

        public async Task<List<string>> SubscribeLearningPlanAndRelationshipsAsync1(string id)
        {
            // await graph.Cypher
            //     .Match("(user:User)", "(lp:LearningPlan)")
            //     .Where((User user) => user.UserId == learningPlanSubscriptionWrapper.UserId)
            //     .AndWhere((LearningPlanWrapper lp) => lp.LearningPlanId == learningPlanSubscriptionWrapper.LearningPlanId)
            //     .Create("(user)-[g:Subscribe_LP]->(lp)")
            //     .ExecuteWithoutResultsAsync();

            var response = new List<string>(await graph.Cypher
                .Match("(user:User)-[g:Subscribe_LP]->(lp:LearningPlan)")
                .Where((User user) => user.UserId == id)
                .Return<string>("lp.LearningPlanId")
                .ResultsAsync);
            return response;
        }
        public async Task<List<string>> PopularLearningPlanAndRelationshipsAsync1(string techName)
        {
            techName = techName.ToUpper();
            var response = new List<string>(await graph.Cypher
                .Match($"(lp:LearningPlan)-[:TEACHES]->(t:Technology {{Name:'{techName}' }})")
                
                .Return<string>("lp.LearningPlanId")
                .OrderBy("lp.TotalSubscribers DESC, lp.AverageRating DESC")
                .ResultsAsync);
            return response;
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
                string intensityStringForWith =
                    "k.Intensity as kI,"
                    + "co.Intensity as coI,"
                    + "ap.Intensity as apI,"
                    + "an.Intensiy as anI,"
                    + "s.Intensity as sI,"
                    + "e.Intensity as eI";
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
                            $"(u)-[k:Knowledge]->(:Concept {{Name: '{concept.Name}' }})",
                            $"(u)-[co:Comprehension]->(:Concept {{Name: '{concept.Name}' }})",
                            $"(u)-[ap:Application]->(:Concept {{Name: '{concept.Name}' }})",
                            $"(u)-[an:Analysis]->(:Concept {{Name: '{concept.Name}' }})",
                            $"(u)-[s:Synthesis]->(:Concept {{Name: '{concept.Name}' }})",
                            $"(u)-[e:Evaluation]->(:Concept {{Name: '{concept.Name}' }})"
                        )
                        .With(intensityStringForWith)
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
