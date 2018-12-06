using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class LearningPlan
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public List<string> Domain { get; set; }
        public List<Resource> Resources { get; set; }
    }
}