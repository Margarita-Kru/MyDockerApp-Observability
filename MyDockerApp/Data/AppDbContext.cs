using Microsoft.EntityFrameworkCore;
using MyDockerApp.Entities;

namespace MyDockerApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions) { }

    public DbSet<WeatherRecord> WeatherRecords { get; set; }
}
