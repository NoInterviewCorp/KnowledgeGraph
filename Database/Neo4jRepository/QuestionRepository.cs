using SME.Models;
using SME.Services;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;

namespace SME.Persistence
{
    public class QuestionRepository //: IQuestionRepository
    {
        private GraphClient graph;
        public QuestionRepository(GraphDbConnection graph)
        {
            this.graph = graph.Client;
        }

        // // Question
        // public async Task<List<Question>> AddQuestionsAsync(List<Question> questions)
        // {
        //     //     var query = graph.Cypher;
        //     //     var questionParams = new ExpandoObject() as IDictionary<string, object>;
        //     //     for (int i = 0; i < questions.Count; i++)
        //     //     {
        //     //         var question = questions[i];
        //     //         if (question.QuestionId == null || question.QuestionId == "")
        //     //         {
        //     //             question.QuestionId = Guid.NewGuid().ToString("N");
        //     //         }
        //     //         questionParams.Add("question"+i,question);
        //     //         query = query
        //     //             .Merge("(q:Question)")
        //     //             .Where((Question q) => q.QuestionId == question.QuestionId)
        //     //             .OnCreate()
        //     //             .Set($"q={{question{i}}}")
        //     //             .WithParams(questionParams)
        //     //             .Return(q => q.As<Question>());
        //     //     }
        //     //     var result = await query;
        //     //     var list = new List<Question>(result);
        //     //     return (list.Count == 0) ? null : list;
        //     return null;
        // }

        public async Task<List<Question>> GetQuestionsAsync()
        {
            return new List<Question>(
                       await graph.Cypher
                   .Match("(q:Question)")
                   .ReturnDistinct(q => q.As<Question>())
                   .ResultsAsync
            );
        }
        public async Task<List<Question>> GetQuestionsByConceptOfATechAsync(string technology, string concept)
        {
            var result = await graph.Cypher
                .Match("(t:Technology)-[:EXPLAINS]->(c:Concept)-[:TESTS]->(q:Question)")
                .Where((Technology t, Concept c) => t.Name == technology && c.Name == concept)
                .Return(q => q.As<Question>())
                .ResultsAsync;
            var list = new List<Question>(result);
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                return list;
            }
        }
        // public async Task<Question> UpdateQuestionAsync(Question question)
        // {
        //     return null;
        // }
        public async Task<bool> DeleteQuestionByIdAsync(string QuestionId)
        {
            var result = new List<Question>(await graph.Cypher
                .Match("(q:Question {QuestionId:{id}})")
                .WithParams(new
                {
                    id = QuestionId
                })
                .Return(q => q.As<Question>())
                .ResultsAsync);
            if (result.Count == 0)
            {
                return false;
            }
            await graph.Cypher
                .OptionalMatch("(q:Question)-[relation]->()")
                .Where((Question q) => q.QuestionId.ToString() == QuestionId)
                .Delete("q, relation")
                .ExecuteWithoutResultsAsync();
            return true;
        }

        // public async Task<List<Question>> GetQuestionsAsync(){
        //     return null;
        // }
    }
}