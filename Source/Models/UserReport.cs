using System.Collections.Generic;
using KnowledgeGraph.Models;

namespace KnowledeGraph.Models
{
    public class UserReport
    {
        public string UserId { get; set; }
        public List<TechnologyReport> TechnologyReports { get; set; }
        
    }
}