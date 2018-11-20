using System.Collections.Generic;

namespace KnowledgeGraph.Database.Models
{
    public class Question
    {
        public int Id {get;set;}
        public string ProblemStatement{get;set;}
        public List<Option> Options;
    }
}