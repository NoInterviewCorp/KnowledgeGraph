using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class LearningPlanWrapper
    {
        public string LearningPlanId { get; set; }
        public string AuthorId { get; set; }
        public int Subscriber { get; set; }
        public float AvgRating { get; set; }
        public TechnologyWrapper Technology { get; set; }
        public List<ResourceWrapper> Resources { get; set; }
    }
}