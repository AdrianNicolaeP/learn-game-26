using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LearnGame26.Pages
{
    public class IndexModel : PageModel
    {
        public int? Scor { get; set; }

        public void OnGet()
        {
            Scor = TempData["Scor"] as int?;
        }
    }
}
