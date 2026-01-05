using Microsoft.EntityFrameworkCore;
using PharmacyProject.Models;
namespace PharmacyProject.BridgeData
{
    public class Bridge : DbContext
    {
        public Bridge(DbContextOptions<Bridge> options) : base(options)
        {
        }
        public DbSet<Prop> PropTable { get; set; }
        public DbSet<ClientProp> UserData { get; set; }

    }
}
