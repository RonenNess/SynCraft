using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Persons;

public class CreateModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public CreateModel(SynCraftDbContext db) => _db = db;

    [BindProperty]
    public Person Person { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        _db.Persons.Add(Person);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
