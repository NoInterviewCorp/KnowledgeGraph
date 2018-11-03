using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace MyProfile{
    public class User{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserId{get;set;}
        public string UserName{get;set;}
        public string FirstName{get;set;}
        public string LastName{get;set;}
        public byte[] ProfilePic{get;set;}
        public string Description{get;set;}
    }
}