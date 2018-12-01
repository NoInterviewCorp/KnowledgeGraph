using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace My_Profile
{
    public class Resource
    {
        public string ResourceId { get; set; }
        public string RDescription { get; set; }
        public float AvgRating { get; set; }
    }
}
