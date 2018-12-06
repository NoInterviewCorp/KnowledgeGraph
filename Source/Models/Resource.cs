using System.Collections.Generic;

namespace KnowledgeGraph.Models
{
    public class Resource
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public List<Question> Questions;
    }
}