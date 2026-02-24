using Microsoft.EntityFrameworkCore;
using SupportTicketAPI.Models;
namespace SupportTicketAPI.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Role> Roles {  get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketStatusLog> TicketStatusLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<TicketComment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TicketComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketStatusLog>()
                .HasOne(l => l.Ticket)
                .WithMany(t => t.StatusLogs)
                .HasForeignKey(l => l.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

 
            modelBuilder.Entity<TicketStatusLog>()
                .HasOne(l => l.ChangedBy)
                .WithMany()
                .HasForeignKey(l => l.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = RoleName.Manager },
                new Role { Id = 2, Name = RoleName.Support },
                new Role { Id = 3, Name = RoleName.User }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
