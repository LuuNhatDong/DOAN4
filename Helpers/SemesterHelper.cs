using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuanLyThucTap.Models;

namespace QuanLyThucTap.Helpers
{
    public static class SemesterHelper
    {
        public const string CookieKey = "SelectedDotThucTapId";

        public static async Task<long> GetSelectedSemesterIdAsync(HttpContext httpContext, ThucTapDbContext db)
        {
            if (httpContext.Request.Cookies.TryGetValue(CookieKey, out var cookieVal) && long.TryParse(cookieVal, out var parsedId))
            {
                if (await db.DotThucTaps.AnyAsync(d => d.Id == parsedId))
                {
                    return parsedId;
                }
            }

            // Mặc định lấy đợt đang kích hoạt
            var activeDot = await db.DotThucTaps.FirstOrDefaultAsync(d => d.TrangThaiKichHoat);
            if (activeDot != null)
            {
                return activeDot.Id;
            }

            // Nếu không có đợt kích hoạt, lấy đợt mới nhất
            var latestDot = await db.DotThucTaps.OrderByDescending(d => d.Id).FirstOrDefaultAsync();
            return latestDot?.Id ?? 0;
        }

        public static async Task<DotThucTap?> GetSelectedSemesterAsync(HttpContext httpContext, ThucTapDbContext db)
        {
            var id = await GetSelectedSemesterIdAsync(httpContext, db);
            if (id == 0) return null;
            return await db.DotThucTaps.FindAsync(id);
        }
    }
}
