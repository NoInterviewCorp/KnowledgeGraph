using System.Collections.Generic;
using KnowledgeGraph.Models;

namespace KnowledgeGraph.Database
{
    public class ResourceWrapper
    {
        // public ResourceWrapper(Resource resource)
        // {
        //     ResourceId = resource.ResourceId;
        //     BloomLevel = resource.BloomLevel;
        //     Questions = new List<QuestionWrapper>();
        //     Concepts = new List<ConceptWrapper>();
        //     Technologies = new List<TechnologyWrapper>();
        //     foreach (Question q in resource.Questions)
        //     {
        //         Questions.Add(new QuestionWrapper(q));
        //     }
        //     foreach (Concept c in resource.Concepts)
        //     {
        //         Concepts.Add(new ConceptWrapper(c));
        //     }
        //     foreach (Technology t in resource.Technologies)
        //     {
        //         Technologies.Add(new TechnologyWrapper(t));
        //     }

        // }
        public string ResourceId { get; set; }
        public BloomTaxonomy BloomLevel { get; set; }
        public List<QuestionWrapper> Questions { get; set; }
        public List<ConceptWrapper> Concepts { get; set; }
        public List<TechnologyWrapper> Technologies { get; set; }
    }
}