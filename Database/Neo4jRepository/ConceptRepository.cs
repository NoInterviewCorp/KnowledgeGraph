using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;
using KnowledgeGraph.Services;

namespace KnowledgeGraph.Database
{
    public class ConceptRepository // : IConceptRepository
    {
        private IGraphClient graph;
        public ConceptRepository(GraphDbConnection graph)
        {
            this.graph = graph.graph;
        }

        // Concept

        public async Task<ConceptWrapper> AddConceptAsync(ConceptWrapper concept)
        {

            // Converting resource/technology/concept names to upper case
            // for faster indexed searches
            concept.Name = concept.Name.ToUpper();
            return new List<ConceptWrapper>(await graph.Cypher
                  .Merge("(con:Concept { Name: {conceptName} })")
                  .OnCreate()
                  .Set("con={concept}")
                  .WithParams(new
                  {
                      conceptName = concept.Name,
                      concept
                  })
                  .Return(con => con.As<ConceptWrapper>())
                  .ResultsAsync)[0];
        }
        public async Task<List<ConceptWrapper>> GetConceptsAsync()
        {
            return new List<ConceptWrapper>(
                await graph.Cypher
                    .Match("(c:Concept)")
                    .Return(c => c.As<ConceptWrapper>())
                    .ResultsAsync
            );
        }
        public async Task<ConceptWrapper> GetConceptByNameAsync(string name)
        {
            name = name.ToUpper();
            var results = new List<ConceptWrapper>(
                await graph.Cypher
                    .Match("(c:Concept)")
                    .Where((ConceptWrapper c) => c.Name == name)
                    .Return(c => c.As<ConceptWrapper>())
                    .ResultsAsync
            );
            return (results.Count == 0) ? null : results[0];
        }
        // public async Task<Concept> UpdateConceptAsync(Concept concept)
        // {
        //     // Converting resource/technology/concept names to upper case
        //     // for faster indexed searches
        //     concept.Name = concept.Name.ToUpper();
        //     return new List<Concept>(await graph.Cypher
        //           .Merge("(con:Concept { Name: {conceptName} })")
        //           .OnMatch()
        //           .Set("con={concept}")
        //           .WithParams(new
        //           {
        //               conceptName = concept.Name,
        //               concept
        //           })
        //           .Return(con => con.As<Concept>())
        //           .ResultsAsync)[0];
        // }

        public async Task<bool> DeleteConceptByNameAsync(string name)
        {
            name = name.ToUpper();
            var result = new List<ConceptWrapper>(await graph.Cypher
                .Match("(c:Concept {Name:{conceptName}})")
                .WithParams(new{
                    conceptName = name
                })
                .Return(c=>c.As<ConceptWrapper>())
                .ResultsAsync);
            if(result.Count == 0){
                return false;
            }
            await graph.Cypher
                .OptionalMatch("(c:Concept)-[relation]->()")
                .Where((ConceptWrapper c) => c.Name == name)
                .Delete("c, relation")
                .ExecuteWithoutResultsAsync();
            return true;
        }
    }
}