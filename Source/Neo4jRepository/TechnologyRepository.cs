using KnowledgeGraph.Services;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;

namespace KnowledgeGraph.Database
{
    public class TechnologyRepository // : ITechnologyRepository
    {
        private IGraphClient graph;
        public TechnologyRepository(GraphDbConnection graph)
        {
            this.graph = graph.graph;
        }

        public async Task<List<TechnologyWrapper>> GetAllTechnologiesAsync()
        {
            return new List<TechnologyWrapper>(
                       await graph.Cypher
                   .Match("(r:Resource)")
                   .ReturnDistinct(t => t.As<TechnologyWrapper>())
                   .ResultsAsync
            );
        }
        public async Task<TechnologyWrapper> AddTechnologyAsync(TechnologyWrapper technology)
        {
            technology.Name = technology.Name.ToUpper();
            var query = new List<TechnologyWrapper>(
                await graph.Cypher
                .Merge("(t:Technology { TechnologyId: {techName} })")
                .OnCreate()
                .Set("t={{technology}}")
                .WithParams(new
                {
                    techName = technology.Name,
                    technology
                })
                .Return(t => t.As<TechnologyWrapper>())
                .ResultsAsync
            );
            return (query.Count == 0) ? null : query[0];
        }
        public async Task<TechnologyWrapper> GetTechnologyByNameAsync(string name)
        {
            name = name.ToUpper();
            var results = new List<TechnologyWrapper>(
                await graph.Cypher
                    .Match("(t:Technology)")
                    .Where((TechnologyWrapper t) => t.Name == name)
                    .Return(t => t.As<TechnologyWrapper>())
                    .ResultsAsync
            );
            return (results.Count == 0) ? null : results[0];
        }
        // public async Task<TechnologyWrapper> UpdateTechnologyAsync(Technology technology)
        // {
        //     return null;
        // }
        public async Task<bool> DeleteTechnologyAsync(string name)
        {
            var result = new List<TechnologyWrapper>(await graph.Cypher
                .Match("(t:Technology {Name:{name}})")
                .WithParams(new
                {
                    name
                })
                .Return(t => t.As<TechnologyWrapper>())
                .ResultsAsync);
            if (result.Count == 0)
            {
                return false;
            }
            await graph.Cypher
                .OptionalMatch("(t:Technology)-[relation]->()")
                .Where((TechnologyWrapper t) => t.Name == name)
                .Delete("t, relation")
                .ExecuteWithoutResultsAsync();
            return true;
        }
    }
}