using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class LearningPlanWrapper
    {
        public string LearningPlanId { get; set; }
        public string AuthorId { get; set; }
        public int TotalSubscribers { get; set; }
        public float AverageRating { get; set; }
        public TechnologyWrapper Technology { get; set; }
        public List<ResourceWrapper> Resources { get; set; }
    }
}