using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;

namespace STO.Web.Endpoints;

public static class InventoryEndpoints
{
    public static RouteGroupBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory");

        group.MapGet("/character/{characterId:int}", async (int characterId, IInventoryService service) =>
            Results.Ok(await service.GetByCharacterIdAsync(characterId)));

        group.MapGet("/character/{characterId:int}/location/{location}", async (int characterId, InventoryLocation location, IInventoryService service) =>
            Results.Ok(await service.GetByLocationAsync(characterId, location)));

        group.MapGet("/search", async (string q, IInventoryService service) =>
            Results.Ok(await service.SearchAcrossAccountsAsync(q)));

        group.MapPost("/", async (InventoryItem item, IInventoryService service) =>
        {
            var created = await service.AddItemAsync(item);
            return Results.Created($"/api/inventory/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async (int id, InventoryItem item, IInventoryService service) =>
        {
            item.Id = id;
            var updated = await service.UpdateItemAsync(item);
            return Results.Ok(updated);
        });

        group.MapDelete("/{id:int}", async (int id, IInventoryService service) =>
            await service.RemoveItemAsync(id) ? Results.NoContent() : Results.NotFound());

        return group;
    }
}
