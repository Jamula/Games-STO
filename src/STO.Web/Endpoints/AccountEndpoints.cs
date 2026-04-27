using STO.Core.Models;
using STO.Core.Services;

namespace STO.Web.Endpoints;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapGet("/", async (IAccountService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:int}", async (int id, IAccountService service) =>
            await service.GetByIdAsync(id) is { } account
                ? Results.Ok(account)
                : Results.NotFound());

        group.MapGet("/by-gamertag/{gamertag}", async (string gamertag, IAccountService service) =>
            await service.GetByGamertagAsync(gamertag) is { } account
                ? Results.Ok(account)
                : Results.NotFound());

        group.MapPost("/", async (Account account, IAccountService service) =>
        {
            var created = await service.CreateAsync(account);
            return Results.Created($"/api/accounts/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async (int id, Account account, IAccountService service) =>
        {
            account.Id = id;
            var updated = await service.UpdateAsync(account);
            return Results.Ok(updated);
        });

        group.MapDelete("/{id:int}", async (int id, IAccountService service) =>
            await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        return group;
    }
}
