namespace MoreSpeakers.Web.Authorization;

public static class AppRoles
{
    // Core admin role (already seeded in the system)
    public const string Administrator = "Administrator";

    // Future least-privilege roles (documented; may be seeded later)
    public const string UserManager = "UserManager";
    public const string CatalogManager = "CatalogManager";
    public const string Reporter = "Reporter";
    public const string Moderator = "Moderator";
}
