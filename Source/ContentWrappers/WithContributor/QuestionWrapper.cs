using System.Collections.Generic;
using KnowledgeGraph.Models;

namespace KnowledgeGraph.Database
{
    public class QuestionWrapper
    {
        // public QuestionWrapper(Question q)
        // {
        //     QuestionId = q.QuestionId;
        //     BloomLevel = q.BloomLevel;
        //     Concepts = new List<ConceptWrapper>();
        //     foreach (Concept c in q.Concepts)
        //     {
        //         Concepts.Add(new ConceptWrapper(c));
        //     }
        //     Technology = new TechnologyWrapper(q.Technology);
        // }

        public string QuestionId { get; set; }
        public BloomTaxonomy BloomLevel { get; set; }
        public List<ConceptWrapper> Concepts { get; set; }
        public TechnologyWrapper Technology { get; set; }
    }
}