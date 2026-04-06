using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class DbModel : DbContext
    {
        public DbModel(DbContextOptions<DbModel> options) : base(options) { }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ToDoModel> ToDos { get; set; }
    }
}