using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class Question
    {
        public string Id {get;set;}
        public string ProblemStatement{get;set;}
        public List<Option> Options;
    }
}