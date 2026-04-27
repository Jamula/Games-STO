using STO.Core.Models;
using STO.Core.Services;

namespace STO.Web.Endpoints;

public static class BuildEndpoints
{
    public static RouteGroupBuilder MapBuildEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/builds").WithTags("Builds");

        group.MapGet("/active", async (IBuildService service) =>
            Results.Ok(await service.GetActiveBuildsAsync()));

        group.MapGet("/{id:int}", async (int id, IBuildService service) =>
            await service.GetByIdAsync(id) is { } build
                ? Results.Ok(build)
                : Results.NotFound());

        group.MapPost("/", async (Build build, IBuildService service) =>
        {
            var created = await service.CreateAsync(build);
            return Results.Created($"/api/builds/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async (int id, Build build, IBuildService service) =>
        {
            build.Id = id;
            var updated = await service.UpdateAsync(build);
            return Results.Ok(updated);
        });

        group.MapDelete("/{id:int}", async (int id, IBuildService service) =>
            await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        // Slots
        group.MapPost("/{buildId:int}/slots", async (int buildId, BuildSlot slot, IBuildService service) =>
        {
            slot.BuildId = buildId;
            var created = await service.AddSlotAsync(slot);
            return Results.Created($"/api/builds/{buildId}/slots/{created.Id}", created);
        });

        group.MapPut("/slots/{slotId:int}", async (int slotId, BuildSlot slot, IBuildService service) =>
        {
            slot.Id = slotId;
            var updated = await service.UpdateSlotAsync(slot);
            return Results.Ok(updated);
        });

        group.MapDelete("/slots/{slotId:int}", async (int slotId, IBuildService service) =>
            await service.RemoveSlotAsync(slotId) ? Results.NoContent() : Results.NotFound());

        // Nested route: builds by character
        app.MapGet("/api/characters/{id:int}/builds", async (int id, IBuildService service) =>
            Results.Ok(await service.GetByCharacterIdAsync(id)))
            .WithTags("Builds");

        return group;
    }
}
