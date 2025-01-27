using form_backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace form_backend.Services.Context {

    public class UserDb : DbContext 
    {
        public UserDb(DbContextOptions<UserDb> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<FormModel> FormModels => Set<FormModel>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().HasData(
                new User{ ID = 1, Email = "test@test.com", Hash="hashPass", Salt = "saltPass", IsAdmin = true, SubmitTime = DateTime.Now}
            );
        }
    }
}