using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace My_Profile
{
    public class Question
    {
        public string QuestionId { get; set; }
        public string Technology { get; set; }
        public int Total_Ambiguity { get; set; }
    }
}
