namespace Lanka.IntegrationTests.Bloggers;

internal static class BloggerData
{
    public static DateOnly ValidBirthDate => DateOnly.FromDateTime(
        DateTime.Now.AddYears(-18).AddDays(-1)
    );

    public static DateOnly InvalidBirthDate => DateOnly.FromDateTime(DateTime.Today);
}
