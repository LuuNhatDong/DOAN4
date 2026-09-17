using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Forwarded Headers cho Render / Reverse Proxy HTTPS
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// 1. Cấu hình DbContext kết nối Supabase PostgreSQL
builder.Services.AddDbContext<ThucTapDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// 2. Cấu hình Cookie Authentication và Google OAuth
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddGoogle(options =>
    {
        var googleConfig = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") 
            ?? googleConfig["ClientId"] 
            ?? "282374742719-sk3d0ekojrn0eicno6hgqkm4n06aj8ma.apps.googleusercontent.com";
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") 
            ?? googleConfig["ClientSecret"] 
            ?? "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J";
        options.CallbackPath = "/signin-google";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("GiangVienOnly", policy => policy.RequireRole("giang_vien"));
    options.AddPolicy("SinhVienOnly", policy => policy.RequireRole("sinh_vien"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Cấu hình Render / Cloud Container Port
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 3. Cấu hình Routing cho Areas (Admin, GiangVien, SinhVien)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Tự động nạp dữ liệu di trú (Migration seed) 57 sinh viên và 9 giảng viên CTUET
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ThucTapDbContext>();
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
        {
            conn.Open();
        }

        var migrationFile06 = Path.Combine(app.Environment.ContentRootPath, "supabase", "migrations", "06_seed_57_sinh_vien_khoa_cntt.sql");
        if (File.Exists(migrationFile06))
        {
            using var cmd06 = conn.CreateCommand();
            cmd06.CommandText = File.ReadAllText(migrationFile06);
            cmd06.ExecuteNonQuery();
            Console.WriteLine("--> [DATABASE] Da nap thanh cong 57 Sinh Vien 4 nganh & 9 Giang Vien BM HTTT (CTUET)!");
        }

        var migrationFile07 = Path.Combine(app.Environment.ContentRootPath, "supabase", "migrations", "07_update_features_and_thuyanh.sql");
        if (File.Exists(migrationFile07))
        {
            using var cmd07 = conn.CreateCommand();
            cmd07.CommandText = File.ReadAllText(migrationFile07);
            cmd07.ExecuteNonQuery();
            Console.WriteLine("--> [DATABASE] Da chay migration 07: ThS. Nguyen Thuy Anh & Du lieu theo doi tien do!");
        }

        var migrationFile08 = Path.Combine(app.Environment.ContentRootPath, "supabase", "migrations", "08_add_file_and_drive_to_bao_cao_dinh_ky.sql");
        if (File.Exists(migrationFile08))
        {
            using var cmd08 = conn.CreateCommand();
            cmd08.CommandText = File.ReadAllText(migrationFile08);
            cmd08.ExecuteNonQuery();
            Console.WriteLine("--> [DATABASE] Da chay migration 08: Bo sung file_bao_cao_url va link_drive vao bao_cao_dinh_ky!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("--> [DATABASE SEED ERROR]: " + ex.Message);
    }
}

app.Run();
