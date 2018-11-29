using KnowledgeGraph.Services;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;
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
                    .Match("(u:User{UserId:\"" + authorId + "\"})")
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
                Console.WriteLine("----------------LP->RESOURCE----------------------------");
                Console.WriteLine(resourceQuery.Query.QueryText);
                await resourceQuery.ExecuteWithoutResultsAsync();
                return lpResult;
            }
            catch (Exception e)
            {
                Console.WriteLine("----------------------EXCEPTION-MESSAGE------------------------------------");
                Console.WriteLine(e.Message);
                Console.WriteLine("----------------------STACK-TRACE-----------------------------------------");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("-------------------------INNER-EXCEPTION-----------------------------");
                Console.WriteLine(e.InnerException);
                return null;
            }
            // return null;
        }

        // public void CreateQuestionsAndRelationships(ResourceWrapper resource)
        // {
        //     throw new System.NotImplementedException();
        // }

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

            if(questions.Count != 0){
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
                    Console.WriteLine("---------------------QUESTION-CREATE-----------------------------");
                    Console.WriteLine(questionQuery.Query.QueryText);
                    await questionQuery.ExecuteWithoutResultsAsync();
                    Console.WriteLine("---------------------QUESTION->CONCEPT------------------------------------");
                    Console.WriteLine(questionConceptQuery.Query.QueryText);
                    await questionConceptQuery.ExecuteWithoutResultsAsync();
                    Console.WriteLine("---------------------QUESTION->TECHNOLOGY---------------------------------");
                    Console.WriteLine(questionTechQuery.Query.QueryText);
                    await questionTechQuery.ExecuteWithoutResultsAsync();
                }
            }
            return resQuery;
        }


    }
}