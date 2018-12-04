using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KnowledgeGraph.Models
{

    public class LearningPlanFeedBack
    {
        public int LearningPlanFeedBackId { get; set; }
        public string LearningPlanId { get; set; }
        public string UserId { get; set; }
        public int Star { get; set; }
        public int Subscribe { get; set; }
    }
}
