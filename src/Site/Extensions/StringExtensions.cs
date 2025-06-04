namespace UmbracoMemberGroupSegments.Extensions;

public static class StringExtensions
{
    public static string AsSegmentAlias(this string roleName)
        => roleName.ToLowerInvariant().ReplaceNonAlphanumericChars('-');
}