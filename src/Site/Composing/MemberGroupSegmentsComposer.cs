using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using UmbracoMemberGroupSegments.Middleware;
using UmbracoMemberGroupSegments.Services;

namespace UmbracoMemberGroupSegments.Composing;

public class MemberGroupSegmentsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // replace the default segment service with the custom implementation 
        builder.Services.AddUnique<ISegmentService, MemberGroupSegmentService>();

        // ensure that segmentation is enabled
        builder.Services.Configure<SegmentSettings>(settings => settings.Enabled = true);

        // add the middleware for contextualizing logged-in members
        builder.Services.AddScoped<MemberGroupSegmentMiddleware>();
        builder.Services.Configure<UmbracoPipelineOptions>(options =>
            options.AddFilter(new UmbracoPipelineFilter(nameof(MemberGroupSegmentMiddleware))
            {
                PostPipeline = app => app.UseMiddleware<MemberGroupSegmentMiddleware>()
            }));

        // configure the Delivery API
        // - ensure it's enabled
        // - enable member auth is enabled and available through Swagger UI
        // - see also https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/protected-content-in-the-delivery-api
        builder.Services.Configure<DeliveryApiSettings>(config =>
        {
            config.Enabled = true;
            config.MemberAuthorization = new()
            {
                AuthorizationCodeFlow = new()
                {
                    Enabled = true,
                    LoginRedirectUrls = new HashSet<Uri> { new("https://localhost:44331/umbraco/swagger/oauth2-redirect.html") },
                }
            };
        });
        builder.Services.ConfigureOptions<Umbraco.Cms.Api.Delivery.Configuration.ConfigureUmbracoMemberAuthenticationDeliveryApiSwaggerGenOptions>();

        // configure the default login path to something that actually exists as routable content
        builder.Services.Configure<CookieAuthenticationOptions>(
            IdentityConstants.ApplicationScheme,
            options => options.LoginPath = "/login"
        );
    }
}