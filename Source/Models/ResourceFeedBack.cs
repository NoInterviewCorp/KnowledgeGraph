using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeGraph.Models
{
    public class ResourceFeedBack
    {

        public int ResourceFeedBackId { get; set; }
        public string ResourceId { get; set; }
        public string UserId { get; set; }
        public int Star { get; set; }

    }
}
