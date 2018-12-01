using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace My_Profile
{
    public class QuestionFeedBack
    {

       public int QuestionFeedBackId { get; set; }
        public string QuestionId { get; set; }
        public string UserId { get; set; }
        public int Ambiguity { get; set; }

    }
}
