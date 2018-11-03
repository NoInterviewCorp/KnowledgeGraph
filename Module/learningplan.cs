using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace MyProfile{
    public class LearningPlan{
        public string LearningPlanId{get;set;}
        public string UserId{get;set;}
    }
}
