using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace my_profile{
    public class User{
       
        public int UserId{get;set;}
        public string UserName{get;set;}
        public string FirstName{get;set;}
        public string LastName{get;set;}
        public string Description{get;set;}
       
    }
}