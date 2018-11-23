using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace my_profile{
    public class ResourceProgress{
        public int ResourceProgressId{get;set;}
        public int ResourceId {get ; set;}
        public int UserId{ get; set;}
        public bool isCheck{get;set;}
       
    }
}
