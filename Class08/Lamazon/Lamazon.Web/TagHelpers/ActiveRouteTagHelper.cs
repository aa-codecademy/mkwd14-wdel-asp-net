using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Lamazon.Web.TagHelpers;

[HtmlTargetElement(Attributes = "is-active-route")]
public class ActiveRouteTagHelper : TagHelper
{
    [HtmlAttributeName("asp-controller")]
    public string? Controller { get; set; }

    [HtmlAttributeName("asp-action")]
    public string? Action { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();

        if (IsCurrentRoute(currentController, currentAction))
        {
            output.AddClass("active", HtmlEncoder.Default);
        }

        output.Attributes.RemoveAll("is-active-route");
    }

    private bool IsCurrentRoute(string? currentController, string? currentAction)
    {
        if (string.IsNullOrWhiteSpace(Controller) || !string.Equals(Controller, currentController, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return string.IsNullOrWhiteSpace(Action) || string.Equals(Action, currentAction, StringComparison.OrdinalIgnoreCase);
    }
}
