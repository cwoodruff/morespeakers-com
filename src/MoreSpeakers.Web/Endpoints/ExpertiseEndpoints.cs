using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Endpoints;

/// <summary>
/// Endpoints for expertise
/// </summary>
[Produces("application/json")]
public static class ExpertiseEndpoints
{
    public static void MapExpertiseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/Expertises");
        group.MapGet("searchSectors", SearchSectors).AllowAnonymous();
        group.MapGet("searchExpertiseCategories", SearchExpertiseCategories).AllowAnonymous();
        group.MapGet("searchExpertises", SearchExpertises).AllowAnonymous();
    }

    private static async Task<IResult> SearchSectors([FromQuery(Name = "q")] string searchTerm, ISectorManager sectorManager)
    {
        var sectors = await sectorManager.GetAllSectorsAsync(searchTerm: searchTerm, includeCategories: true);
        return Results.Json(sectors, GetSerializeOptions);
    }
    
    private static async Task<IResult> SearchExpertiseCategories([FromQuery(Name = "q")] string searchTerm, IExpertiseManager expertiseManager)
    {
        var expertiseCategoriesResult = await expertiseManager.GetAllCategoriesAsync(searchTerm: searchTerm);
        return expertiseCategoriesResult.IsSuccess
            ? Results.Json(expertiseCategoriesResult.Value, GetSerializeOptions)
            : Results.Problem(
                title: "Unable to search expertise categories.",
                detail: expertiseCategoriesResult.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError);
    }
    
    private static async Task<IResult> SearchExpertises([FromQuery(Name = "q")] string searchTerm, IExpertiseManager expertiseManager)
    {
        var expertisesResult = await expertiseManager.GetAllExpertisesAsync(searchTerm: searchTerm);
        return expertisesResult.IsSuccess
            ? Results.Json(expertisesResult.Value, GetSerializeOptions)
            : Results.Problem(
                title: "Unable to search expertises.",
                detail: expertisesResult.Error.Message,
                statusCode: StatusCodes.Status500InternalServerError);
    }

    private static JsonSerializerOptions GetSerializeOptions =>
        new(JsonSerializerDefaults.Web) { ReferenceHandler = ReferenceHandler.Preserve };
}
