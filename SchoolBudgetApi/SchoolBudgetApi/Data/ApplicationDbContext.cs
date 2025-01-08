using Microsoft.EntityFrameworkCore;
using SchoolBudgetApi.Models;

namespace SchoolBudgetApi.Data;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options){
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<User> Users => Set<User>();
}