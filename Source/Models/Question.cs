using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class Question
    {
        public string QuestionId { get; set; }
        public string ProblemStatement { get; set; }
        public List<Option> Options;
    }
}