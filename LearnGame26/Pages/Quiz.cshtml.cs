using LearnGame26.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace LearnGame26.Pages
{
    public class QuizModel : PageModel
    {
        private readonly ILogger<QuizModel> _logger;
        private List<Question> questions;

        [BindProperty]
        public string? Intrebare { get; set; }
        [BindProperty]
        public string? VarA { get; set; }
        [BindProperty]
        public string? VarB { get; set; }
        [BindProperty]
        public string? VarC { get; set; }
        [BindProperty]
        public int Raspuns { get; set; }
        [BindProperty]
        public int RaspunsCorect { get; set; }
        [BindProperty]
        public int Vieti { get; set; }
        [BindProperty]
        public int IntrebariCorecte { get; set; }
        [BindProperty]
        public int TimpRamas { get; set; }
        [BindProperty]
        public string? Feedback { get; set; }
        [BindProperty]
        public bool APrimitViata { get; set; }

        public QuizModel(ILogger<QuizModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string? set, bool reset = false)
        {
            _logger.LogInformation("OnGet called with set = {Set}, reset = {Reset}", set, reset);

            try
            {
                questions = set switch
                {
                    "1" => QuestionList1.GetQuestions(),
                    "2" => QuestionList2.GetQuestions(),
                    "3" => QuestionList3.GetQuestions(),
                    _ => QuestionList1.GetQuestions()
                };

                if (reset || !HttpContext.Session.Keys.Contains("QuestionIndex"))
                {
                    _logger.LogInformation("Resetting game state");
                    Vieti = 3;
                    IntrebariCorecte = 0;  // Resetăm numărul de răspunsuri corecte
                    TimpRamas = 60;

                    questions.Shuffle();
                    HttpContext.Session.SetString("Questions", JsonSerializer.Serialize(questions));
                    HttpContext.Session.SetInt32("QuestionIndex", 0);
                    HttpContext.Session.SetInt32("IntrebariCorecte", 0); // Resetăm scorul
                    HttpContext.Session.SetInt32("Vieti", Vieti);
                    HttpContext.Session.SetInt32("TimpRamas", TimpRamas);
                }
                else
                {
                    Vieti = HttpContext.Session.GetInt32("Vieti") ?? 3;
                    IntrebariCorecte = HttpContext.Session.GetInt32("IntrebariCorecte") ?? 0;
                    TimpRamas = HttpContext.Session.GetInt32("TimpRamas") ?? 60;

                    var json = HttpContext.Session.GetString("Questions");
                    if (!string.IsNullOrEmpty(json))
                    {
                        questions = JsonSerializer.Deserialize<List<Question>>(json);
                    }
                }

                GenerateNewQuestion();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing OnGet");
                throw;
            }
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("OnPost called with Raspuns = {Raspuns}", Raspuns);

            try
            {
                var json = HttpContext.Session.GetString("Questions");
                if (!string.IsNullOrEmpty(json))
                {
                    questions = JsonSerializer.Deserialize<List<Question>>(json);
                }

                Vieti = HttpContext.Session.GetInt32("Vieti") ?? 3;
                IntrebariCorecte = HttpContext.Session.GetInt32("IntrebariCorecte") ?? 0;

                Intrebare = HttpContext.Session.GetString("Intrebare");
                VarA = HttpContext.Session.GetString("VarA");
                VarB = HttpContext.Session.GetString("VarB");
                VarC = HttpContext.Session.GetString("VarC");
                RaspunsCorect = HttpContext.Session.GetInt32("RaspunsCorect") ?? 0;

                if (Raspuns == RaspunsCorect)
                {
                    IntrebariCorecte++;
                    Feedback = "Corect!";
                    APrimitViata = false;

                    if (IntrebariCorecte % 10 == 0)
                    {
                        Vieti++;
                        APrimitViata = true;
                    }
                }
                else
                {
                    Vieti--;
                    Feedback = $"Greșit! Răspunsul corect era: {GetAnswerText(RaspunsCorect)}";
                    if (Vieti <= 0)
                    {
                        HttpContext.Session.SetInt32("Scor", IntrebariCorecte);
                        TempData["Scor"] = IntrebariCorecte;
                        TempData["Message"] = "Ați rămas fără vieți!";
                        return RedirectToPage("Index");
                    }
                }

                HttpContext.Session.SetInt32("Vieti", Vieti);
                HttpContext.Session.SetInt32("IntrebariCorecte", IntrebariCorecte);

                GenerateNewQuestion();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing OnPost");
                throw;
            }
        }

        private void GenerateNewQuestion()
        {
            _logger.LogInformation("GenerateNewQuestion called");

            try
            {
                int questionIndex = HttpContext.Session.GetInt32("QuestionIndex") ?? 0;
                if (questionIndex >= questions.Count)
                {
                    _logger.LogInformation("No more questions, redirecting to index.");
                    TempData["Scor"] = IntrebariCorecte;
                    TempData["Message"] = "Nu mai sunt întrebări!";
                    HttpContext.Session.SetInt32("Scor", IntrebariCorecte);
                    Response.Redirect("/Index");
                    return;
                }

                var question = questions[questionIndex];
                Intrebare = question.Text;
                VarA = question.VarA;
                VarB = question.VarB;
                VarC = question.VarC;
                RaspunsCorect = question.RaspunsCorect;

                HttpContext.Session.SetInt32("QuestionIndex", questionIndex + 1);
                HttpContext.Session.SetString("Intrebare", Intrebare);
                HttpContext.Session.SetString("VarA", VarA);
                HttpContext.Session.SetString("VarB", VarB);
                HttpContext.Session.SetString("VarC", VarC);
                HttpContext.Session.SetInt32("RaspunsCorect", RaspunsCorect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating a new question");
                throw;
            }
        }

        private string GetAnswerText(int raspunsCorect)
        {
            return raspunsCorect switch
            {
                1 => VarA ?? string.Empty,
                2 => VarB ?? string.Empty,
                3 => VarC ?? string.Empty,
                _ => string.Empty
            };
        }
    }
}
