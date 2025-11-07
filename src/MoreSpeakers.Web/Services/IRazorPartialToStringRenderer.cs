namespace MoreSpeakers.Web.Services;

public interface IRazorPartialToStringRenderer
{
    Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model);
}
