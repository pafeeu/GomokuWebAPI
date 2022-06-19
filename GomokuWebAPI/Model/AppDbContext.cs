using GomokuWebAPI.Authentication;
using GomokuWebAPI.Common;
using GomokuWebAPI.Model.Entities;
using GomokuWebAPI.Model.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GomokuWebAPI.Model
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<long>, long>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Move> Moves { get; set; }
        public DbSet<GamePlayer> GamePlayers { get; set; }

        //public DatabaseFacade database => Database; //it's neccessery?

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetSoftDeleteFilter(nameof(Status));
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            //relations
            modelBuilder.Entity<AppUser>()
                .HasOne(u => u.Player).WithOne(p => p.User).IsRequired(false)
                .HasForeignKey<AppUser>(u => u.PlayerId);

            base.OnModelCreating(modelBuilder);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTimeOffset.UtcNow;
                        entry.Entity.Status = Status.Active;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTimeOffset.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.Entity.LastModifiedDate = DateTimeOffset.UtcNow;
                        entry.Entity.InactivatedDate = DateTimeOffset.UtcNow;
                        entry.Entity.Status = Status.Deleted;
                        entry.State = EntityState.Modified;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
