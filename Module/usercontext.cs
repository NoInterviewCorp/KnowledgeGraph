using Microsoft.EntityFrameworkCore;
namespace MyProfile
{
    public class UserContext : DbContext
    {
        public DbSet<User> Use { get; set; }
        public DbSet<LearningPlan> LP { get; set; }
        public DbSet<ResourceProgress> RP { get; set; }
      //  public DbSet<CourseContent> CC { get; set; }
        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=myprofiledatabasec;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.Entity<User>().HasMany(n => n.learningPlans).WithOne().HasPrincipalKey(c => c.UserId);
            modelBuilder.Entity<LearningPlan>().HasMany(n => n.ResourceProgresses).WithOne().HasPrincipalKey(c => c.LearningPlanId);
           
           
        }  
    }
}