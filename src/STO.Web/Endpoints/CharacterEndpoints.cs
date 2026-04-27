using STO.Core.Models;
using STO.Core.Services;

namespace STO.Web.Endpoints;

public static class CharacterEndpoints
{
    public static RouteGroupBuilder MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/characters").WithTags("Characters");

        group.MapGet("/", async (ICharacterService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id:int}", async (int id, ICharacterService service) =>
            await service.GetByIdAsync(id) is { } character
                ? Results.Ok(character)
                : Results.NotFound());

        group.MapGet("/{id:int}/details", async (int id, ICharacterService service) =>
            await service.GetWithFullDetailsAsync(id) is { } character
                ? Results.Ok(character)
                : Results.NotFound());

        group.MapPost("/", async (Character character, ICharacterService service) =>
        {
            var created = await service.CreateAsync(character);
            return Results.Created($"/api/characters/{created.Id}", created);
        });

        group.MapPut("/{id:int}", async (int id, Character character, ICharacterService service) =>
        {
            character.Id = id;
            var updated = await service.UpdateAsync(character);
            return Results.Ok(updated);
        });

        group.MapDelete("/{id:int}", async (int id, ICharacterService service) =>
            await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        // Nested route: characters by account
        app.MapGet("/api/accounts/{id:int}/characters", async (int id, ICharacterService service) =>
            Results.Ok(await service.GetByAccountIdAsync(id)))
            .WithTags("Characters");

        return group;
    }
}
