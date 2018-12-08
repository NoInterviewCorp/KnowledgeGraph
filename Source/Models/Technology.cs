using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KnowledgeGraph.Models
{
    public class Technology
    {
        public string Name { get; set; }
        public List<Concept> Concepts { get; set; }
    }
}