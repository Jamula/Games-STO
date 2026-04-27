using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace STO.Data.Context;

public class StoDbContextFactory : IDesignTimeDbContextFactory<StoDbContext>
{
    public StoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StoDbContext>();
        optionsBuilder.UseSqlite("Data Source=sto.db");
        return new StoDbContext(optionsBuilder.Options);
    }
}
