using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MoreSpeakers.Web.Services;

public class RazorPartialToStringRenderer : IRazorPartialToStringRenderer
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;

    public RazorPartialToStringRenderer(
        IHttpContextAccessor httpContextAccessor,
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
    }

    public async Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is null");
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor());
        var partial = FindView(actionContext, partialName);
        await using var output = new StringWriter();
        ViewDataDictionary<TModel> viewDataDictionary =
            new(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        TempDataDictionary tempDataDictionary = new(actionContext.HttpContext, _tempDataProvider);
        viewDataDictionary.Model = model;
        
        var viewContext = new ViewContext(
            actionContext,
            partial,
            viewDataDictionary,
            tempDataDictionary,
            output,
            new HtmlHelperOptions()
        );
        
        await partial.RenderAsync(viewContext);
        return output.ToString();
    }

    private IView FindView(ActionContext actionContext, string partialName)
    {
        var getPartialResult = _viewEngine.GetView(null, partialName, false);
        if (getPartialResult.Success)
        {
            return getPartialResult.View;
        }

        var findPartialResult = _viewEngine.FindView(actionContext, partialName, false);
        if (findPartialResult.Success)
        {
            return findPartialResult.View;
        }

        var searchedLocations = getPartialResult.SearchedLocations.Concat(findPartialResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find partial '{partialName}'. The following locations were searched:" }.Concat(
                searchedLocations));
        ;
        throw new InvalidOperationException(errorMessage);
    }
}