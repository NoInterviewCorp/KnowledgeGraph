using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace My_Profile
{
    public class GiveStarPayload
    {
        public int Rating { get; set; }
        public int Subscribe { get; set; }
        public int Ambigous { get; set; }
    }
}
