using System.Collections.Generic;

namespace KnowledgeGraph.Models {
    public class Resource {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public List<Concept> Concepts { get; set; }
        public List<Technology> Technologies { get; set; }
        public List<Question> Questions;
    }
}