using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Persons;

public class IndexModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public IndexModel(SynCraftDbContext db) => _db = db;

    public List<Person> Persons { get; set; } = [];

    public async Task OnGetAsync()
    {
        Persons = await _db.Persons.OrderBy(p => p.Name).ToListAsync();
    }
}
