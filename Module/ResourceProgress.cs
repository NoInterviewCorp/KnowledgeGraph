using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace my_profile{
    public class ResourceProgress{
        public string ResourceProgressId{get;set;}

        public bool isCheck{get;set;}
       
    }
}
