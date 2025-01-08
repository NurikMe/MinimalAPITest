using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SchoolBudgetApi.Data;
using Bogus;
using SchoolBudgetApi.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/investment", async (ApplicationDbContext dbCon, [Required] InvestmentDto invDto) =>
{
    using var transaction = await dbCon.Database.BeginTransactionAsync();
    try
    {
        var user = await dbCon.Users.FirstAsync(u => u.Id == invDto.UserId);
        await dbCon.Investments.AddAsync(new()
        {
            Description = invDto.Description,
            EshopLink = invDto.EshopLink,
            IsReoccuring = invDto.IsReoccuring,
            Cost = invDto.Cost,
            IsPending = invDto.IsPending,
            UserId = user.Id,
            User = user
        });

        await dbCon.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }

    return Results.Created();
});

app.MapPut("/investments/{id}", async (ApplicationDbContext dbCon, int id, [Required] InvestmentDto invDto) =>
{
    using var transaction = await dbCon.Database.BeginTransactionAsync();
    try
    {
        await dbCon.Investments.Where(inv => inv.Id == id)
                    .ExecuteUpdateAsync(setter => setter.SetProperty(inv => inv.Description, invDto.Description)
                                                        .SetProperty(inv => inv.EshopLink, invDto.EshopLink)
                                                        .SetProperty(inv => inv.Cost, invDto.Cost)
                                                        .SetProperty(inv => inv.IsPending, invDto.IsPending)
                                                        .SetProperty(inv => inv.IsReoccuring, invDto.IsReoccuring));

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
    return Results.Accepted();
});

app.MapDelete("/investments/{id}", async (ApplicationDbContext dbCon, int id) =>
{
    using var transaction = await dbCon.Database.BeginTransactionAsync();
    try
    {
        await dbCon.Investments.Where(inv => inv.Id == id)
                    .ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
    return Results.NoContent();
});

app.MapGet("/investments", (ApplicationDbContext dbCon,
                            [FromQuery] int? userId,
                            [FromQuery] bool? IsPending) =>
{
    var query = dbCon.Investments.AsQueryable();

    if (userId is not null) { query = query.Where(inv => inv.UserId == userId); }
    if (IsPending is not null) { query = query.Where(inv => inv.IsPending == IsPending); }

    return query.Select(inv => new {inv.Description,
                                    inv.EshopLink,
                                    inv.IsReoccuring,
                                    inv.Cost,
                                    User = new {inv.User.FirstName,
                                                inv.User.LastName} });
});

app.MapPost("/fill", async (ApplicationDbContext dbCon) =>
{
    using var transaction = await dbCon.Database.BeginTransactionAsync();
    try
    {
        var userFaker = new Faker<User>()
        .RuleFor(u => u.FirstName, f => f.Person.FirstName)
        .RuleFor(u => u.LastName, f => f.Person.LastName);
        var user = userFaker.Generate(5);
        await dbCon.Users.AddRangeAsync(user);
        await dbCon.SaveChangesAsync();

        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
});

app.Run();

record InvestmentDto(string Description, string EshopLink, bool IsReoccuring, decimal Cost, bool? IsPending, int UserId);

