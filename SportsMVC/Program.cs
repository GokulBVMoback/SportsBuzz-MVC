using Entities.Models;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddControllersWithViews().AddNewtonsoftJson
    (options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore).AddNewtonsoftJson
    (options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

builder.Services.AddMvc()
.AddNewtonsoftJson(options =>
       options.SerializerSettings.ContractResolver =
          new CamelCasePropertyNamesContractResolver());


//builder.Configuration.AddJsonFile("appsettings.json", optional: true);
//builder.Services.Configure<APIDetails>(builder.Configuration.GetSection("APIDetails"));


//builder.Services.Configure(IApplicationBuilder app, IWebHostEnvironment env)
//{
//    app.Use(async (context, next) =>
//    {
//        ITempDataDictionaryFactory factory = context.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
//        ITempDataDictionary tempData = factory.GetTempData(context);
//        //get or set data in tempData
//        var TestData = tempData["Token"];
//        // Do work that doesn't write to the Response.
//        await next.Invoke();
//        // Do logging or other work that doesn't write to the Response.
//    });
//}

//builder.Services.AddHttpClient<IHttpClientFactory, someClient>();
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(
    option =>
    {
        option.LoginPath = "/User/LogIn";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });




builder.Services.AddHttpClient("someClient", c =>
{
    c.BaseAddress = new Uri("https://localhost:7191/api/");
      c.DefaultRequestHeaders.Add("Authorization", "token");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=LogIn}/{id?}");
app.MapRazorPages();
app.Run();