using STO.Core.Models;
using STO.Core.Services;

namespace STO.Web.Endpoints;

public static class ValuableItemEndpoints
{
    public static RouteGroupBuilder MapValuableItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/valuables").WithTags("Valuable Items");

        group.MapGet("/", async (IValuableItemService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/account/{accountId:int}", async (int accountId, IValuableItemService service) =>
            Results.Ok(await service.GetByAccountIdAsync(accountId)));

        group.MapGet("/search", async (string q, IValuableItemService service) =>
            Results.Ok(await service.SearchAsync(q)));

        group.MapGet("/summary", async (IValuableItemService service) =>
            Results.Ok(await service.GetSummaryAsync()));

        group.MapPost("/", async (ValuableItem item, IValuableItemService service) =>
        {
            var created = await service.AddAsync(item);
            return Results.Created($"/api/valuables/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async (int id, ValuableItem item, IValuableItemService service) =>
        {
            item.Id = id;
            var updated = await service.UpdateAsync(item);
            return Results.Ok(updated);
        });

        group.MapDelete("/{id:int}", async (int id, IValuableItemService service) =>
            await service.RemoveAsync(id) ? Results.NoContent() : Results.NotFound());

        return group;
    }
}
