using STO.Core.Models;
using STO.Core.Services;

namespace STO.Web.Endpoints;

public static class ReputationEndpoints
{
    public static RouteGroupBuilder MapReputationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reputation").WithTags("Reputation");

        group.MapGet("/character/{characterId:int}", async (int characterId, IReputationService service) =>
            Results.Ok(await service.GetByCharacterIdAsync(characterId)));

        group.MapGet("/character/{characterId:int}/summary", async (int characterId, IReputationService service) =>
            Results.Ok(await service.GetReputationSummaryAsync(characterId)));

        group.MapGet("/account/{accountId:int}", async (int accountId, IReputationService service) =>
            Results.Ok(await service.GetAllForAccountAsync(accountId)));

        group.MapPut("/", async (Reputation reputation, IReputationService service) =>
        {
            var updated = await service.UpdateAsync(reputation);
            return Results.Ok(updated);
        });

        return group;
    }
}
