using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Persons;

public class DeleteModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public DeleteModel(SynCraftDbContext db) => _db = db;

    public Person Person { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var person = await _db.Persons.FindAsync(id);
        if (person == null)
            return NotFound();

        Person = person;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var person = await _db.Persons.FindAsync(id);
        if (person != null)
        {
            _db.Persons.Remove(person);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
