var builder = WebApplication.CreateBuilder(args);

// Adãugare servicii necesare
builder.Services.AddRazorPages();

// Adãugare servicii pentru sesiuni
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // timpul de expirare a sesiunii
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurare logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

var app = builder.Build();

// Configurare pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Adãugare middleware pentru sesiuni
app.UseSession();

app.MapRazorPages();

app.Run();
    