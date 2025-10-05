using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Web.Data;
using MoreSpeakers.Web.Models;

namespace MoreSpeakers.Web.Services;

public class ExpertiseService(ApplicationDbContext context) : IExpertiseService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Expertise>> GetAllExpertiseAsync()
    {
        return await _context.Expertise
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Expertise?> GetExpertiseByIdAsync(int id)
    {
        return await _context.Expertise.FindAsync(id);
    }

    public async Task<IEnumerable<Expertise>> SearchExpertiseAsync(string searchTerm)
    {
        return await _context.Expertise
            .Where(e => e.Name.Contains(searchTerm) ||
                        (e.Description != null && e.Description.Contains(searchTerm)))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<bool> CreateExpertiseAsync(string name, string? description = null)
    {
        try
        {
            var expertise = new Expertise
            {
                Name = name,
                Description = description
            };

            _context.Expertise.Add(expertise);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateExpertiseAsync(int id, string name, string? description = null)
    {
        try
        {
            var expertise = await _context.Expertise.FindAsync(id);
            if (expertise != null)
            {
                expertise.Name = name;
                expertise.Description = description;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteExpertiseAsync(int id)
    {
        try
        {
            var expertise = await _context.Expertise.FindAsync(id);
            if (expertise != null)
            {
                _context.Expertise.Remove(expertise);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Expertise>> GetPopularExpertiseAsync(int count = 10)
    {
        return await _context.Expertise
            .Include(e => e.UserExpertise)
            .OrderByDescending(e => e.UserExpertise.Count)
            .Take(count)
            .ToListAsync();
    }
}