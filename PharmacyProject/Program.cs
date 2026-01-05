using Microsoft.EntityFrameworkCore;
using PharmacyProject.BridgeData;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// 1. تعريف الخدمات (Services)
// ==============================
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<Bridge>(option =>option.UseSqlServer(builder.Configuration.GetConnectionString("PropPortal")));

// تفعيل خدمة السشن (مكانها هون ممتاز)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // مدة بقاء السلة
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ==============================
// 2. إعداد الـ Pipeline (الترتيب مهم!)
// ==============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");   
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ---------------------------------------------------------
// تفعيل السشن (لازم تكون بعد Routing وقبل Authorization)
// ---------------------------------------------------------
app.UseSession(); // <--- هاد هو المكان الصح 100%

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pharmacy}/{action=login}/{id?}");

// تشغيل التطبيق (نهاية الملف)
app.Run();