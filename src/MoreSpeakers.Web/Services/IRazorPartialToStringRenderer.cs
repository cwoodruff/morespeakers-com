namespace MoreSpeakers.Web.Services;

public interface IRazorPartialToStringRenderer
{
    Task<string> RenderPartialToStringAsync<TModel>(HttpContext httpContext, string partialName, TModel model);
}
