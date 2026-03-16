using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Persons;

public class EditModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public EditModel(SynCraftDbContext db) => _db = db;

    [BindProperty]
    public Person Person { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var person = await _db.Persons.FindAsync(id);
        if (person == null)
            return NotFound();

        Person = person;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var existing = await _db.Persons.FindAsync(Person.Id);
        if (existing == null)
            return NotFound();

        existing.Name = Person.Name;
        existing.Email = Person.Email;
        existing.Role = Person.Role;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
