using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
public class LocationContext : DbContext
{
    public DbSet<Cm_CustomerLocations> CustomerLocations { get; set; }
    public DbSet<Cm_Customer> Customers { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);        
        optionsBuilder.UseSqlServer("Server=tcp:10.211.xx.x,xxxx;Initial Catalog=Customer;User ID=sa;Password=xxxxxx;");
    }
}