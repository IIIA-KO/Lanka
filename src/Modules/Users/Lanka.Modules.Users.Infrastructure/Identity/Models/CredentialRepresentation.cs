namespace Lanka.Modules.Users.Infrastructure.Identity.Models;

internal sealed record CredentialRepresentation(string Type, string Value, bool Temporary);
