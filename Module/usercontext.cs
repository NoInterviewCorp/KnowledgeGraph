using Microsoft.EntityFrameworkCore;
namespace MyProfile
{
    public class UserContext : DbContext
    {
        public DbSet<User> Use { get; set; }
         //public DbSet<LearningPlan> LP { get; set; }
       // public DbSet<TopicLink> TL { get; set; }
      //  public DbSet<CourseContent> CC { get; set; }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=myprofi2;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<User>();
           
        }  
    }
}