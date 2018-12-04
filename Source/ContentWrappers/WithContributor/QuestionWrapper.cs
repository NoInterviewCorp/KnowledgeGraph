using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class QuestionWrapper
    {
        public string QuestionId { get; set; }
        public BloomTaxonomy BloomLevel { get; set; }
        public List<ConceptWrapper> Concepts { get; set; }
        public TechnologyWrapper Technology { get; set; }
    }
}