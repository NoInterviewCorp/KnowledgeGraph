using System.Collections.Generic;

namespace KnowledgeGraph.Database
{
    public class LearningPlanWrapper
    {
        // public LearningPlanWrapper(LearningPlan lp)
        // {
        //     LearningPlanId = lp.LearningPlanId;
        //     AuthorId = lp.AuthorId;
        //     Technology = new TechnologyWrapper(lp.Technology);
        //     Resources = new List<ResourceWrapper>();
        //     foreach (Resource r in lp.Resources)
        //     {
        //         Resources.Add(new ResourceWrapper(r));
        //     }
        // }
        public string LearningPlanId { get; set; }
        public string AuthorId { get; set; }
        public TechnologyWrapper Technology { get; set; }
        public List<ResourceWrapper> Resources { get; set; }
    }
}