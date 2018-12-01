using My_Profile.Services;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;
namespace My_Profile.Persistence
{
    public class GraphFunctions : IGraphFunctions
    {
        private IGraphClient graph;
        public GraphFunctions(GraphDbConnection _graphclient)
        {
            graph = _graphclient.client;
        }

        public async Task RatingLearningPlanAndRelationshipsAsync(LearningPlanFeedBack learningPlanFeedback)
        {
           
                GiveStarPayload giveStar = new GiveStarPayload { Rating = learningPlanFeedback.Star };
                await graph.Cypher
                    .Match("(user:User)", "(lp:LearningPlan)")
                    .Where((User user) => user.UserId == learningPlanFeedback.UserId)
                    .AndWhere((LearningPlan lp) => lp.LearningPlanId == learningPlanFeedback.LearningPlanId)
                    .Merge("(user)-[g:RATING_LP]->(lp)")
                    .OnCreate()
                    .Set("g={giveStar}")
                    .OnMatch()
                    .Set("g={giveStar}")
                    .WithParams(new
                    {
                        userRating = learningPlanFeedback.Star,
                        giveStar
                    })
                    .ExecuteWithoutResultsAsync();
                var LPqueryAvg = new List<float>(await graph.Cypher
                    .Match("(:User)-[g:RATING_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                    .With("lp,  avg(g.Rating) as avg_rating ")
                    .Set("lp.AvgRating = avg_rating")
                    .WithParams(new
                    {
                        id = learningPlanFeedback.LearningPlanId,
                        // rating=
                    })
                    .Return<float>("lp.AvgRating")
                    .ResultsAsync);
              //  var avg_rating = LPqueryAvg[0];
                Console.WriteLine("dasdsa");
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                //  return  Ok(new List<float>(LPqueryAvg)[0]);
            
            // return Ok(new List<float>(LPqueryAvg));
            // throw new NotImplementedException();
        }
        public async Task RatingResourceAndRelationshipsAsync(ResourceFeedBack resourceFeedBack)
        {
        GiveStarPayload giveStar = new GiveStarPayload { Rating = resourceFeedBack.Star };
            await graph.Cypher
                .Match("(user:User)", "(Re:Resource)")
                .Where((User user) => user.UserId == resourceFeedBack.UserId)
                .AndWhere((Resource Re) => Re.ResourceId == resourceFeedBack.ResourceId)
                .Merge("(user)-[g:RATING_Resource]->(Re)")
                .OnCreate()
                .Set("g={giveStar}")
                .OnMatch()
                .Set("g={giveStar}")
                .WithParams(new
                {
                    userRating = resourceFeedBack.Star,
                    giveStar
                })
                .ExecuteWithoutResultsAsync();
            var Re_queryAvg = new List<float>(await graph.Cypher
                .Match("(:User)-[g:RATING_Resource]->(Re:Resource {ResourceId:{id}})")
                .With("Re,  avg(g.Rating) as avg_rating ")
                .Set("Re.AvgRating = avg_rating")
                .WithParams(new
                {
                    id = resourceFeedBack.ResourceId,
                    // rating=
                })
                .Return<float>("Re.AvgRating)")
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                .ResultsAsync);
                 Console.WriteLine("heeh");
           // return Ok(new List<float>(Re_queryAvg)[0]);
        }
        public async Task SubscribeLearningPlanAndRelationshipsAsync(LearningPlanFeedBack learningPlanFeedback)
        {
            GiveStarPayload LearningPlanSubscriber = new GiveStarPayload { Subscribe = learningPlanFeedback.Subscribe };
            await graph.Cypher
                .Match("(user:User)", "(lp:LearningPlan)")
                .Where((User user) => user.UserId == learningPlanFeedback.UserId)
                .AndWhere((LearningPlan lp) => lp.LearningPlanId == learningPlanFeedback.LearningPlanId)

                .Merge("(user)-[g:Subscribe_LP]->(lp)")
                .OnCreate()
                .Set("g={LearningPlanSubscriber}")
                .OnMatch()
                .Set("g={LearningPlanSubscriber}")
                .WithParams(new
                {
                    usersubscribe = learningPlanFeedback.Subscribe,
                    LearningPlanSubscriber
                })
                .ExecuteWithoutResultsAsync();
            var totalsubscriber = new List<int>(await graph.Cypher
               .Match("(:User)-[g:Subscribe_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                // .Match((GiveStarPayload sub)=>sub.Subscribe==1)
                .With("lp,  count(g.Subscribe) as total_subscriber ")
                .Set("lp.Subscriber = total_subscriber")
                .WithParams(new
                {
                    id = learningPlanFeedback.LearningPlanId,
                    // rating=
                })
               .Return<int>("lp.Subscriber")
               
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                .ResultsAsync);
                Console.WriteLine("haha");
         //   return Ok(new List<int>(totalsubscriber)[0]);
        }
         public async Task UnSubscribeLearningPlanAndRelationshipsAsync(LearningPlanFeedBack learningPlanFeedback)
        {
            GiveStarPayload LearningPlanSubscriber = new GiveStarPayload { Subscribe = learningPlanFeedback.Subscribe };
            await graph.Cypher
                .Match("(user:User)", "(lp:LearningPlan)")
                .Where((User user) => user.UserId == learningPlanFeedback.UserId)
                .AndWhere((LearningPlan lp) => lp.LearningPlanId == learningPlanFeedback.LearningPlanId)

                .Merge("(user)-[g:Subscribe_LP]->(lp)")
                .Delete("g")
                .ExecuteWithoutResultsAsync();
            var totalsubscriber = await graph.Cypher
               .Match("(:User)-[g:Subscribe_LP]->(lp:LearningPlan {LearningPlanId:{id}})")
                // .Match((GiveStarPayload sub)=>sub.Subscribe==1)
                .With("lp,  count(g.Subscribe) as total_subscriber ")
                .Set("lp.Subscriber = total_subscriber")
                .WithParams(new
                {
                    id = learningPlanFeedback.LearningPlanId,
                    // rating=
                })
               .Return<int>("lp.Subscriber")
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                .ResultsAsync;
            //return Ok(new List<int>(totalsubscriber)[0]);
        }
        public async Task ReportQuestionAndRelationshipsAsync(QuestionFeedBack questionFeedBack)
        {
         GiveStarPayload QuestionReport = new GiveStarPayload { Ambigous = questionFeedBack.Ambiguity };
            await graph.Cypher
                .Match("(user:User)", "(qe:Question)")
                .Where((User user) => user.UserId == questionFeedBack.UserId)
                .AndWhere((Question qe) => qe.QuestionId == questionFeedBack.QuestionId)

                .Merge("(user)-[g:Report_Question]->(qe)")
                .OnCreate()
                .Set("g={QuestionReport}")
                .OnMatch()
                .Set("g={QuestionReport}")
                .WithParams(new
                {
                    userreport = questionFeedBack.Ambiguity,
                    QuestionReport
                })
                .ExecuteWithoutResultsAsync();
            var totalReport = await graph.Cypher
               .Match("(:User)-[g:Report_Question]->(qe:Question {QuestionId:{id}})")
                // .Match((GiveStarPayload sub)=>sub.Subscribe==1)
                .With("qe,  count(g.ambigous) as total_ambiguity ")
                .Set("qe.Total_Ambiguity = total_ambiguity")
                .WithParams(new
                {
                    id = questionFeedBack.QuestionId,
                    // rating=
                })
               .Return<int>("qe.Total_Ambiguity")
                // .Return (g => Avg(g.As<GiveStarPayload>().Rating))
                .ResultsAsync;
           // return Ok(new List<int>(totalReport)[0]);
        }
        
    }
}