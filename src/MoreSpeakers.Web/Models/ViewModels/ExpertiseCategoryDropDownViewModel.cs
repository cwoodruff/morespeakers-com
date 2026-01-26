using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models.ViewModels;

public class ExpertiseCategoryDropDownViewModel
{
    public List<ExpertiseCategory> ExpertiseCategories { get; set; } = [];
    public int SelectedCategoryId { get; set; }
}