using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using UmbracoMemberGroupSegments.Extensions;

namespace UmbracoMemberGroupSegments.Middleware;

public class MemberGroupSegmentMiddleware : IMiddleware
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IMemberManager _memberManager;
    private readonly ILogger<MemberGroupSegmentMiddleware> _logger;

    public MemberGroupSegmentMiddleware(
        IUmbracoContextAccessor umbracoContextAccessor,
        IVariationContextAccessor variationContextAccessor,
        IMemberManager memberManager,
        ILogger<MemberGroupSegmentMiddleware> logger)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _variationContextAccessor = variationContextAccessor;
        _memberManager = memberManager;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await Contextualize();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to contextualize the request.");
        }
        await next(context);
    }

    private async Task Contextualize()
    {
        if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext) is false)
        {
            return;
        }

        // is it a frontend request?
        if (umbracoContext.IsFrontEndUmbracoRequest() is false)
        {
            // nope - is it a Content Delivery API request?
            if (umbracoContext.CleanedUmbracoUrl.PathAndQuery.StartsWith("/umbraco/delivery/api/v2/content") is false)
            {
                // none of the above - probably a backoffice request, don't mess with it
                return;
            }
        }

        var member = await _memberManager.GetCurrentMemberAsync();
        if (member is null)
        {
            return;
        }

        var role = (await _memberManager.GetRolesAsync(member)).FirstOrDefault();
        if (role is null)
        {
            return;
        }

        // set the variation context segment while retaining any already resolved culture
        _variationContextAccessor.VariationContext = new VariationContext(
            culture: _variationContextAccessor.VariationContext?.Culture,
            segment: role.AsSegmentAlias()
        );
    }
}