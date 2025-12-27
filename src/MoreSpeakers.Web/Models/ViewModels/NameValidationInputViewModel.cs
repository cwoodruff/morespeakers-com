namespace MoreSpeakers.Web.Models.ViewModels;

public class NameValidationInputViewModel
{
    /// <summary>
    /// The value of the input
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// The component's input name for validation
    /// </summary>
    public string InputName { get; set; } = "";
    /// <summary>
    /// The endpoint URL for name validation
    /// </summary>
    /// <remarks>
    /// Provide the portion of the Url after the base address and api endpoint for name validation
    /// Example: if the url is localhost:7001/api/expertises/searchExpertises, the endpoint would be /expertises/searchExpertises
    /// </remarks>
    public string SearchEndPointUrl { get; set; } = "";
    /// <summary>
    /// The placeholder text for the input
    /// </summary>
    public string Placeholder { get; set; } = "";
    
    /// <summary>
    /// The name of the query parameter for the search term
    /// </summary>
    public string SearchParameterName { get; set; } = "q";

}