using KnowledgeGraph.Services;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;

namespace KnowledgeGraph.Database
{
    public class QuestionRepository //: IQuestionRepository
    {
        private IGraphClient graph;
        public QuestionRepository(GraphDbConnection graph)
        {
            this.graph = graph.graph;
        }

        // // Question
        // public async Task<List<QuestionWrapper>> AddQuestionsAsync(List<QuestionWrapper> questions)
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
        //     //             .Return(q => q.As<QuestionWrapper>());
        //     //     }
        //     //     var result = await query;
        //     //     var list = new List<QuestionWrapper>(result);
        //     //     return (list.Count == 0) ? null : list;
        //     return null;
        // }

        public async Task<List<QuestionWrapper>> GetQuestionsAsync()
        {
            return new List<QuestionWrapper>(
                       await graph.Cypher
                   .Match("(q:Question)")
                   .ReturnDistinct(q => q.As<QuestionWrapper>())
                   .ResultsAsync
            );
        }
        public async Task<List<QuestionWrapper>> GetQuestionsByConceptOfATechAsync(string technology, string concept)
        {
            var result = await graph.Cypher
                .Match("(t:Technology)-[:EXPLAINS]->(c:Concept)-[:TESTS]->(q:Question)")
                .Where((TechnologyWrapper t, ConceptWrapper c) => t.Name == technology && c.Name == concept)
                .Return(q => q.As<QuestionWrapper>())
                .ResultsAsync;
            var list = new List<QuestionWrapper>(result);
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                return list;
            }
        }
        // public async Task<QuestionWrapper> UpdateQuestionAsync(Question question)
        // {
        //     return null;
        // }
        public async Task<bool> DeleteQuestionByIdAsync(string QuestionId)
        {
            var result = new List<QuestionWrapper>(await graph.Cypher
                .Match("(q:Question {QuestionId:{id}})")
                .WithParams(new
                {
                    id = QuestionId
                })
                .Return(q => q.As<QuestionWrapper>())
                .ResultsAsync);
            if (result.Count == 0)
            {
                return false;
            }
            await graph.Cypher
                .OptionalMatch("(q:Question)-[relation]->()")
                .Where((QuestionWrapper q) => q.QuestionId.ToString() == QuestionId)
                .Delete("q, relation")
                .ExecuteWithoutResultsAsync();
            return true;
        }

        // public async Task<List<QuestionWrapper>> GetQuestionsAsync(){
        //     return null;
        // }
    }
}