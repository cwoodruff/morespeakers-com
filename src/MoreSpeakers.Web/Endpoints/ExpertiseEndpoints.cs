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
        var expertiseCategories = await expertiseManager.GetAllCategoriesAsync(searchTerm: searchTerm);
        return Results.Json(expertiseCategories, GetSerializeOptions);
    }
    
    private static async Task<IResult> SearchExpertises([FromQuery(Name = "q")] string searchTerm, IExpertiseManager expertiseManager)
    {
        var expertises = await expertiseManager.GetAllExpertisesAsync(searchTerm: searchTerm);
        return Results.Json(expertises, GetSerializeOptions);
    }

    private static JsonSerializerOptions GetSerializeOptions =>
        new(JsonSerializerDefaults.Web) { ReferenceHandler = ReferenceHandler.Preserve };
}