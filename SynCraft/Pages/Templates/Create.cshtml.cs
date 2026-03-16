using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SynCraft.Data;
using SynCraft.Models;

namespace SynCraft.Pages.Templates;

public class CreateModel : PageModel
{
    private readonly SynCraftDbContext _db;

    public CreateModel(SynCraftDbContext db) => _db = db;

    [BindProperty]
    public ProcessTemplate Template { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        Template.CreatedDate = DateTime.UtcNow;
        _db.ProcessTemplates.Add(Template);
        await _db.SaveChangesAsync();
        return RedirectToPage("Edit", new { id = Template.Id });
    }
}
