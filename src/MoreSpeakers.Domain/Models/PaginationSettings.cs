using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Domain.Models;

public class PaginationSettings : IPaginationSettings
{
    public int StartPage { get; set; } = 1;
    public int MinimalPageSize { get; set; } = 20;
    public int MaximizePageSize { get; set; } = 100;
}