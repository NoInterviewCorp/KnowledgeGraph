using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class ResourceWrapper
    {
        public string ResourceId { get; set; }
        public BloomTaxonomy BloomLevel { get; set; }
        public List<QuestionWrapper> Questions { get; set; }
        public List<ConceptWrapper> Concepts { get; set; }
        public List<TechnologyWrapper> Technologies { get; set; }
    }
}