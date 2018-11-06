using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace MyProfile{
    public class ResourceProgress{
        public string ResourceProgressId{get;set;}

        public bool isCheck{get;set;}
        public string LearningPlanId{get;set;}
    }
}
