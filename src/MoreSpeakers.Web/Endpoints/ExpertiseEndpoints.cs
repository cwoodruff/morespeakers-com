using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Endpoints;

/// <summary>
/// Endpoints for expertise
/// </summary>
public static class ExpertiseEndpoints
{
    public static void MapExpertiseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/Expertises");
        group.MapGet("searchSectors", SearchSectors).AllowAnonymous();
        group.MapGet("searchExpertiseCategories", SearchExpertiseCategories).AllowAnonymous();
        group.MapGet("searchExpertises", SearchExpertises).AllowAnonymous();
    }

    private static async Task<IActionResult> SearchSectors([FromQuery] string searchTerm, ISectorManager sectorManager)
    {
        var sectors = await sectorManager.GetAllSectorsAsync(searchTerm: searchTerm, includeCategories: true);
        var json = Serialize(sectors);
        return new JsonResult(json);
    }
    
    private static async Task<JsonResult> SearchExpertiseCategories([FromQuery] string searchTerm, IExpertiseManager expertiseManager)
    {
        var expertiseCategories = await expertiseManager.GetAllCategoriesAsync(searchTerm: searchTerm);
        var json = Serialize(expertiseCategories);
        return new JsonResult(json);
    }
    
    private static async Task<JsonResult> SearchExpertises([FromQuery] string searchTerm, IExpertiseManager expertiseManager)
    {
        var expertises = await expertiseManager.GetAllExpertisesAsync(searchTerm: searchTerm);
        var json = Serialize(expertises);
        return new JsonResult(json);
    }

    private static string Serialize(object obj) => JsonSerializer.Serialize(obj,
        new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve });
}