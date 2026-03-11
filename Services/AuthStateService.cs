namespace SecretWebsite.Services;

/// <summary>
/// Scoped per Blazor circuit — tracks login state for the current user session.
/// Resets when the browser tab is closed or refreshed.
/// </summary>
public class AuthStateService
{
    public bool IsAdmin { get; set; }
    public string? LoggedInUser { get; set; }
    public string? LoggedInRole { get; set; }
}
