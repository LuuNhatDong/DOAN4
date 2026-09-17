-- =========================================================================
-- ĐỒ ÁN 4: HỆ THỐNG QUẢN LÝ THỰC TẬP DOANH NGHIỆP - KHOA CÔNG NGHỆ THÔNG TIN
-- Script 08: Bổ sung cột lưu File Báo Cáo (PDF) và Link Google Drive vào bao_cao_dinh_ky
-- =========================================================================

-- 1. Thêm các cột lưu đường dẫn file PDF và liên kết Google Drive
ALTER TABLE public.bao_cao_dinh_ky 
ADD COLUMN IF NOT EXISTS file_bao_cao_url VARCHAR(500),
ADD COLUMN IF NOT EXISTS link_drive VARCHAR(500);

-- 2. Thêm ràng buộc số ngày làm việc không được âm và không quá 31 ngày
ALTER TABLE public.bao_cao_dinh_ky
DROP CONSTRAINT IF EXISTS bao_cao_dinh_ky_so_ngay_lam_viec_check;

ALTER TABLE public.bao_cao_dinh_ky
ADD CONSTRAINT bao_cao_dinh_ky_so_ngay_lam_viec_check
CHECK (so_ngay_lam_viec >= 0 AND so_ngay_lam_viec <= 31);
