using System.Collections.Generic;

namespace KnowledgeGraph.Database.Models
{
    public class LearningPlan
    {
        public int PlanId{get;set;}
        public string PlanNAme{get;set;}
        public List<string> Domain{get;set;}
        public List<Resource> Resources{get;set;}
    }
}