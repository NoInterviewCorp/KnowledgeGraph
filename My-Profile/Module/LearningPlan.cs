using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace My_Profile
{
    public class LearningPlan
    {

        public string LearningPlanId { get; set; }
        public string Technology { get; set; }
        public float AvgRating { get; set; }
        public int Subscriber { get; set; }

    }
}
