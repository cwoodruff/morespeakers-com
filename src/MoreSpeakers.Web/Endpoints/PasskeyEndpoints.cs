using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoreSpeakers.Domain.Interfaces;
using DataUser = MoreSpeakers.Data.Models.User;

namespace MoreSpeakers.Web.Endpoints;

public static class PasskeyEndpoints
{
    public static void MapPasskeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/Passkey");

        group.MapPost("creationOptions", GetCreationOptions).RequireAuthorization();
        group.MapPost("register", RegisterPasskey).RequireAuthorization();
        group.MapPost("loginOptions", GetLoginOptions);
        group.MapPost("login", LoginPasskey);
        group.MapDelete("{id}", DeletePasskey).RequireAuthorization();
    }

    private static async Task<IResult> GetCreationOptions(
        ClaimsPrincipal userPrincipal,
        IUserManager userManager,
        SignInManager<DataUser> signInManager)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null) return Results.Unauthorized();

        var options = await signInManager.MakePasskeyCreationOptionsAsync(new()
        {
            Id = user.Id.ToString(),
            Name = user.Email ?? user.UserName ?? "User",
            DisplayName = $"{user.FirstName} {user.LastName}"
        });

        return Results.Content(options, "application/json");
    }

    public class RegisterPasskeyRequest
    {
        public string CredentialJson { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
    }

    private static async Task<IResult> RegisterPasskey(
        [FromBody] RegisterPasskeyRequest request,
        ClaimsPrincipal userPrincipal,
        IUserManager userManager,
        SignInManager<DataUser> signInManager)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null) return Results.Unauthorized();

        // Validate input
        var friendlyName = string.IsNullOrWhiteSpace(request.FriendlyName) ? "My Passkey" : request.FriendlyName.Trim();
        if (friendlyName.Length > 100)
        {
            return Results.BadRequest("Friendly name must be 100 characters or less.");
        }

        var attestation = await signInManager.PerformPasskeyAttestationAsync(request.CredentialJson);
        if (!attestation.Succeeded)
        {
            return Results.BadRequest(attestation.Failure.Message);
        }

        // Explicitly set the friendly name on the passkey object before saving
        // Identity uses this property to serialize into the Data column
        attestation.Passkey.Name = friendlyName;

        // Store credential in Identity (for auth)
        var identityResult = await userManager.AddOrUpdatePasskeyAsync(user, attestation.Passkey);
        if (!identityResult.Succeeded)
        {
            return Results.BadRequest("Failed to save passkey to identity store.");
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetLoginOptions(
        [FromQuery] string? email,
        SignInManager<DataUser> signInManager)
    {
        // Note: email is unused in standard MakePasskeyRequestOptionsAsync call unless doing user verification upfront
        // but we accept it to match common flows if needed later.
        var options = await signInManager.MakePasskeyRequestOptionsAsync(null);
        return Results.Content(options, "application/json");
    }

    public class LoginPasskeyRequest
    {
        public string CredentialJson { get; set; } = string.Empty;
    }

    private static async Task<IResult> LoginPasskey(
        [FromBody] LoginPasskeyRequest request,
        SignInManager<DataUser> signInManager)
    {
        var result = await signInManager.PasskeySignInAsync(request.CredentialJson);
        if (result.Succeeded)
        {
            return Results.Ok();
        }

        return Results.Unauthorized();
    }

    private static async Task<IResult> DeletePasskey(
        string id,
        ClaimsPrincipal userPrincipal,
        IUserManager userManager)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null) return Results.Unauthorized();

        // Remove from Identity store and metadata via Manager logic
        var result = await userManager.RemovePasskeyAsync(user.Id, id);

        if (!result) return Results.NotFound();
        return Results.Ok();
    }
}