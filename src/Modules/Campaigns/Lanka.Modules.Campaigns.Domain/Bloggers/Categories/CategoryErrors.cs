using Lanka.Common.Domain;

namespace Lanka.Modules.Campaigns.Domain.Bloggers.Categories;

public static class CategoryErrors
{
    public static Error TooLong(int maxLength) =>
        Error.Validation("Category.TooLong", $"Category name cannot exceed {maxLength} characters.");
}
