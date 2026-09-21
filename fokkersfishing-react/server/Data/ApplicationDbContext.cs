using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FokkersFishing.Api.Data
{
    /// <summary>
    /// EF Core Identity context. Points at the SAME SQLite schema (users, roles,
    /// user-roles, logins) as the original Blazor app, so existing accounts and
    /// password hashes keep working. IdentityServer's operational tables are no
    /// longer needed (JWT replaces IdentityServer), so this derives from the plain
    /// IdentityDbContext.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new RoleConfiguration());
        }
    }
}
