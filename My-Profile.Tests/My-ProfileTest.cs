using System;
using System.Linq;
using My_Profile;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using My_Profile.Controllers;

namespace My_Profile.Tests
{
     public class MyProfileTest
    {
        private List<User> GetMockDatabase(){
            return new List<User>{
                new User{
                   Id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f932"),
                    UserName="nkumar",
                    FullName="Narendar Kumar",
                    Description="html"
                },
                new User{
                    Id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f933"),
                    UserName="nkumar123",
                    FullName="Rohit Kumar",
                    Description="css"
                }

            };
        }
        [Fact]
        public async Task GetAll_Positive_ListWithEntries()
        {
            var Drepo = new Mock<IUserRepository>();
            List<User> not = GetMockDatabase();
            Drepo.Setup(d => d.GetAllUsers()).Returns(Task.FromResult(not));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            var result =await valuecontroller.Get();
            var okObject = result as OkObjectResult;
            Assert.NotNull(okObject);
            var model = okObject.Value as List<User>;
            Assert.NotNull(model);

            Assert.Equal (not.Count, model.Count);
            
        }
        [Fact]
        public async Task GetAll_Negative_EmptyList()
        {
            var Drepo = new Mock<IUserRepository>();
            List<User> not = new List<User>();
            Drepo.Setup(d => d.GetAllUsers()).Returns(Task.FromResult(not));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            var result = await valuecontroller.Get();
            var okObject = result as OkObjectResult;
            Assert.NotNull(okObject);
            var model = okObject.Value as List<User>;
            Assert.NotNull (model);
            Assert.Equal (not.Count, model.Count); 
        }
         [Fact]
        public async Task GetAll_Negative_DatabaseError () {
            var Drepo = new Mock<IUserRepository>();
            List<User> not = null;
            Drepo.Setup(d => d.GetAllUsers()).Returns(Task.FromResult(not));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            var result =await valuecontroller.Get();
            Assert.IsType<NotFoundObjectResult> (result);
        }
         [Fact]
        public async Task GetById_Positive_ReturnsNoteWithId1()
        {
            var Drepo = new Mock<IUserRepository>();
            List<User> not = GetMockDatabase();
            MongoDB.Bson.ObjectId id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f932");
            Drepo.Setup(d => d.GetUser(id)).Returns(Task.FromResult(not.Find(n => n.Id == id)));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            string id1="5bfbe869c96daa259888f932";
            var result = await valuecontroller.Get(id1);
            var okObject = result as OkObjectResult;
            Assert.NotNull(okObject);
            var model = okObject.Value as User;
            Assert.NotNull (model);

            Assert.Equal (id, model.Id);
        }
         [Fact]
        public async Task GetById_Negative_ReturnsNullNotFound()
        {
            var Drepo = new Mock<IUserRepository>();
            List<User> not = GetMockDatabase();
             MongoDB.Bson.ObjectId id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f934");
            Drepo.Setup(d => d.GetUser(id)).Returns(Task.FromResult(not.Find(n => n.Id == id)));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            string id1="5bfbe869c96daa259888f934";
            var result = await valuecontroller.Get(id1);
            Assert.IsType<NotFoundObjectResult> (result);
        }
         [Fact]
        public async Task PostById_Positive_ReturnsCreated()
        {
           var Drepo = new Mock<IUserRepository>();
            User not = new User
            {
                Id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f935"),
                UserName = "createdpost",
                FullName="naren",
                Description="sdc"
            };
            Drepo.Setup(d => d.PostNote(not)).Returns(Task.FromResult(false));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            var result =await valuecontroller.Post(not);

            var crObject = result as OkObjectResult;
            Assert.NotNull(crObject);

            var model = crObject.Value as User;
            Assert.Equal(not.Id, model.Id);
        }
         [Fact]
         public async Task PostById_Negative_ReturnsBadRequest()
        {
            var Drepo = new Mock<IUserRepository>();
            User not = new User
            {
                Id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f935"),
                UserName = "createdpost",
                FullName="naren",
                Description="sdc"
            };
            Drepo.Setup(d => d.PostNote(not)).Returns(Task.FromResult(true));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            var result =await valuecontroller.Post(not);

            var brObject = result as BadRequestObjectResult;
            Assert.NotNull(brObject);
            
        }
        [Fact]
        public async Task PutById_Positive_ReturnsCreated()
        {
           var Drepo = new Mock<IUserRepository>();
            User not = new User
            {
                Id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f932"),
                UserName = "createdpost",
            };
            MongoDB.Bson.ObjectId id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f932");
           // int id = (int)not.StudentId;
           Drepo.Setup(d => d.FindNote(id)).Returns(Task.FromResult(true));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
            string id1="5bfbe869c96daa259888f932";
            var result =await valuecontroller.Put(id1,not);

            var crObject = result as OkObjectResult;
            Assert.NotNull(crObject);

            var model = crObject.Value as User;
            Assert.Equal(id, model.Id);
        }
         [Fact]
    
        public async Task DeleteById_Positive_ReturnsCreated()
        {
            var Drepo = new Mock<IUserRepository>();
             MongoDB.Bson.ObjectId id=new MongoDB.Bson.ObjectId("5bfbe869c96daa259888f932");
            Drepo.Setup(d => d.FindNote(id)).Returns(Task.FromResult(true));
            ValuesController valuecontroller = new ValuesController(Drepo.Object);
             string id1="5bfbe869c96daa259888f932";
            var result =await valuecontroller.Delete(id1);

            var okObject = result as ObjectResult;
            Assert.NotNull(okObject);
        }
    }
}
