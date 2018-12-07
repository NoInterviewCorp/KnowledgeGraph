using System.Collections.Generic;

namespace KnowledeGraph.Models
{
    public class TechnologyReport
    {
        public string TechnologyName { get; set; }
        public List<ConceptReport> ConceptReports { get; set; }
    }
}