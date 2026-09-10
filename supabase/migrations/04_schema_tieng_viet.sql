-- ==============================================================================
-- HỆ THỐNG QUẢN LÝ THỰC TẬP TỐT NGHIỆP (ĐỒ ÁN 4) - NGÀNH HỆ THỐNG THÔNG TIN
-- Script: Xóa toàn bộ CSDL cũ & Xây dựng lại CSDL mới bằng Tiếng Việt (Supabase / PostgreSQL)
-- ==============================================================================

-- BƯỚC 1: XÓA TOÀN BỘ CƠ SỞ DỮ LIỆU CŨ (DROP TABLES / TYPES CASCADING)
-- ------------------------------------------------------------------------------
DROP TABLE IF EXISTS danh_gia_ket_qua CASCADE;
DROP TABLE IF EXISTS bao_cao_tong_ket CASCADE;
DROP TABLE IF EXISTS bao_cao_dinh_ky CASCADE;
DROP TABLE IF EXISTS de_cuong_thuc_tap CASCADE;
DROP TABLE IF EXISTS phan_cong_huong_dan CASCADE;
DROP TABLE IF EXISTS dot_thuc_tap CASCADE;
DROP TABLE IF EXISTS doanh_nghiep CASCADE;
DROP TABLE IF EXISTS giang_vien CASCADE;
DROP TABLE IF EXISTS sinh_vien CASCADE;
DROP TABLE IF EXISTS tai_khoan CASCADE;

-- Xóa các bảng tiếng Anh cũ (nếu còn tồn tại từ phiên bản trước)
DROP TABLE IF EXISTS weekly_reports CASCADE;
DROP TABLE IF EXISTS final_reports CASCADE;
DROP TABLE IF EXISTS students CASCADE;
DROP TABLE IF EXISTS lecturers CASCADE;
DROP TABLE IF EXISTS companies CASCADE;
DROP TABLE IF EXISTS internship_periods CASCADE;
DROP TABLE IF EXISTS profiles CASCADE;

-- Xóa các kiểu enum cũ nếu có
DROP TYPE IF EXISTS user_role CASCADE;
DROP TYPE IF EXISTS student_status CASCADE;
DROP TYPE IF EXISTS report_status CASCADE;

-- Hàm tự động cập nhật thời gian updated_at
CREATE OR REPLACE FUNCTION cap_nhat_thoi_gian_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


-- ==============================================================================
-- BƯỚC 2: KHỞI TẠO CẤU TRÚC 10 BẢNG DỮ LIỆU CHUẨN TIẾNG VIỆT
-- ==============================================================================

-- 1. Bảng tai_khoan (Xác thực & Phân quyền)
CREATE TABLE tai_khoan (
    id BIGSERIAL PRIMARY KEY,
    ten_dang_nhap VARCHAR(100) UNIQUE NOT NULL,
    mat_khau_hash VARCHAR(255) NOT NULL,
    vai_tro VARCHAR(20) NOT NULL CHECK (vai_tro IN ('admin', 'giang_vien', 'sinh_vien')),
    trang_thai VARCHAR(20) DEFAULT 'hoat_dong' CHECK (trang_thai IN ('hoat_dong', 'khoa')),
    auth_user_id UUID UNIQUE, -- Tùy chọn liên kết với auth.users của Supabase nếu dùng Supabase Auth
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 2. Bảng sinh_vien (Hồ sơ Sinh viên)
CREATE TABLE sinh_vien (
    id BIGSERIAL PRIMARY KEY,
    tai_khoan_id BIGINT UNIQUE REFERENCES tai_khoan(id) ON DELETE CASCADE,
    mssv VARCHAR(20) UNIQUE NOT NULL,
    hovaten VARCHAR(100) NOT NULL,
    lop VARCHAR(20) NOT NULL,
    chuyen_nganh VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    sdt VARCHAR(15),
    gpa DECIMAL(3,2) DEFAULT 0.0 CHECK (gpa >= 0.0 AND gpa <= 4.0),
    trang_thai VARCHAR(20) DEFAULT 'hoat_dong' CHECK (trang_thai IN ('hoat_dong', 'da_an')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 3. Bảng giang_vien (Hồ sơ Giảng viên hướng dẫn)
CREATE TABLE giang_vien (
    id BIGSERIAL PRIMARY KEY,
    tai_khoan_id BIGINT UNIQUE REFERENCES tai_khoan(id) ON DELETE CASCADE,
    magv VARCHAR(20) UNIQUE NOT NULL,
    hovaten VARCHAR(100) NOT NULL,
    hoc_vi VARCHAR(50) NOT NULL,
    bo_mon VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    sdt VARCHAR(15),
    so_luong_toi_da INT DEFAULT 15 CHECK (so_luong_toi_da >= 0),
    trang_thai VARCHAR(20) DEFAULT 'hoat_dong' CHECK (trang_thai IN ('hoat_dong', 'da_an')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 4. Bảng doanh_nghiep (Đối tác tiếp nhận thực tập)
CREATE TABLE doanh_nghiep (
    id BIGSERIAL PRIMARY KEY,
    ma_so_thue VARCHAR(20) UNIQUE NOT NULL,
    ten_doanh_nghiep VARCHAR(255) NOT NULL,
    ten_viet_tat VARCHAR(50) NOT NULL,
    dia_chi TEXT NOT NULL,
    website VARCHAR(255),
    nguoi_lien_he VARCHAR(100),
    sdt_lien_he VARCHAR(15),
    trang_thai VARCHAR(20) DEFAULT 'hoat_dong' CHECK (trang_thai IN ('hoat_dong', 'ngung_hop_tac')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 5. Bảng dot_thuc_tap (Cấu hình học kỳ thực tập)
CREATE TABLE dot_thuc_tap (
    id BIGSERIAL PRIMARY KEY,
    ten_dot VARCHAR(150) NOT NULL,
    nam_hoc VARCHAR(20) NOT NULL,
    ngay_bat_dau DATE NOT NULL,
    ngay_ket_thuc DATE NOT NULL,
    han_dang_ky_cty DATE NOT NULL,
    han_nop_de_cuong DATE NOT NULL,
    trang_thai_kich_hoat BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 6. Bảng phan_cong_huong_dan (Trung tâm kết nối SV - GVHD - Doanh nghiệp)
CREATE TABLE phan_cong_huong_dan (
    id BIGSERIAL PRIMARY KEY,
    sinh_vien_id BIGINT NOT NULL REFERENCES sinh_vien(id) ON DELETE CASCADE,
    giang_vien_id BIGINT REFERENCES giang_vien(id) ON DELETE SET NULL,
    doanh_nghiep_id BIGINT REFERENCES doanh_nghiep(id) ON DELETE SET NULL,
    dot_thuc_tap_id BIGINT NOT NULL REFERENCES dot_thuc_tap(id) ON DELETE CASCADE,
    ten_cty_ngoai VARCHAR(255),
    vi_tri_thuc_tap VARCHAR(100),
    mentor_doanh_nghiep VARCHAR(100),
    sdt_mentor VARCHAR(15),
    loai_chu_ky_bao_cao VARCHAR(20) DEFAULT 'theo_tuan' CHECK (loai_chu_ky_bao_cao IN ('theo_tuan', 'theo_thang')),
    yeu_cau_bao_cao_giua_ky BOOLEAN DEFAULT TRUE,
    trang_thai_duyet VARCHAR(30) DEFAULT 'cho_duyet' CHECK (trang_thai_duyet IN ('cho_duyet', 'da_duyet', 'tu_choi', 'dang_thuc_tap', 'hoan_thanh')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT uq_sinh_vien_dot UNIQUE (sinh_vien_id, dot_thuc_tap_id)
);

-- 7. Bảng de_cuong_thuc_tap (Kế hoạch thực tập)
CREATE TABLE de_cuong_thuc_tap (
    id BIGSERIAL PRIMARY KEY,
    phan_cong_id BIGINT NOT NULL REFERENCES phan_cong_huong_dan(id) ON DELETE CASCADE,
    ten_de_tai VARCHAR(255) NOT NULL,
    muc_tieu TEXT NOT NULL,
    ket_qua_du_kien TEXT NOT NULL,
    cong_nghe_su_dung VARCHAR(255) NOT NULL,
    noi_dung_ke_hoach JSONB NOT NULL DEFAULT '[]'::jsonb,
    nhan_xet_gvhd TEXT,
    trang_thai VARCHAR(30) DEFAULT 'cho_duyet' CHECK (trang_thai IN ('cho_duyet', 'da_duyet', 'yeu_cau_chinh_sua')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 8. Bảng bao_cao_dinh_ky (Nhật ký thực tập theo tuần / tháng)
CREATE TABLE bao_cao_dinh_ky (
    id BIGSERIAL PRIMARY KEY,
    phan_cong_id BIGINT NOT NULL REFERENCES phan_cong_huong_dan(id) ON DELETE CASCADE,
    ky_thu INT NOT NULL,
    ngay_bat_dau DATE NOT NULL,
    ngay_ket_thuc DATE NOT NULL,
    so_ngay_lam_viec INT DEFAULT 5,
    cong_viec_hoan_thanh TEXT NOT NULL,
    kien_thuc_hoc_duoc TEXT NOT NULL,
    kho_khan_vuong_mac TEXT,
    diem_so DECIMAL(3,1) CHECK (diem_so IS NULL OR (diem_so >= 0.0 AND diem_so <= 10.0)),
    nhan_xet_gvhd TEXT,
    nhan_xet_mentor TEXT,
    ngay_nop TIMESTAMPTZ DEFAULT NOW(),
    trang_thai VARCHAR(20) DEFAULT 'da_nop' CHECK (trang_thai IN ('da_nop', 'da_cham', 'tre_han')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT uq_phan_cong_ky UNIQUE (phan_cong_id, ky_thu)
);

-- 9. Bảng bao_cao_tong_ket (Báo cáo Giữa kỳ & Cuối kỳ)
CREATE TABLE bao_cao_tong_ket (
    id BIGSERIAL PRIMARY KEY,
    phan_cong_id BIGINT NOT NULL REFERENCES phan_cong_huong_dan(id) ON DELETE CASCADE,
    loai_bao_cao VARCHAR(20) NOT NULL CHECK (loai_bao_cao IN ('giua_ky', 'cuoi_ky')),
    tieu_de VARCHAR(255) NOT NULL,
    tom_tat TEXT NOT NULL,
    file_bao_cao_url VARCHAR(500) NOT NULL,
    link_github VARCHAR(255),
    diem_so DECIMAL(3,1) CHECK (diem_so IS NULL OR (diem_so >= 0.0 AND diem_so <= 10.0)),
    nhan_xet_gvhd TEXT,
    ngay_nop TIMESTAMPTZ DEFAULT NOW(),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT uq_phan_cong_loai UNIQUE (phan_cong_id, loai_bao_cao)
);

-- 10. Bảng danh_gia_ket_qua (Bảng điểm tổng kết)
CREATE TABLE danh_gia_ket_qua (
    id BIGSERIAL PRIMARY KEY,
    phan_cong_id BIGINT NOT NULL UNIQUE REFERENCES phan_cong_huong_dan(id) ON DELETE CASCADE,
    diem_dinh_ky DECIMAL(3,1) NOT NULL CHECK (diem_dinh_ky >= 0.0 AND diem_dinh_ky <= 10.0),
    diem_giua_ky DECIMAL(3,1) NOT NULL CHECK (diem_giua_ky >= 0.0 AND diem_giua_ky <= 10.0),
    diem_cuoi_ky DECIMAL(3,1) NOT NULL CHECK (diem_cuoi_ky >= 0.0 AND diem_cuoi_ky <= 10.0),
    diem_doanh_nghiep DECIMAL(3,1) NOT NULL CHECK (diem_doanh_nghiep >= 0.0 AND diem_doanh_nghiep <= 10.0),
    diem_tong_ket_he10 DECIMAL(3,2) NOT NULL CHECK (diem_tong_ket_he10 >= 0.0 AND diem_tong_ket_he10 <= 10.0),
    diem_chu VARCHAR(5) NOT NULL CHECK (diem_chu IN ('A+', 'A', 'B+', 'B', 'C+', 'C', 'D+', 'D', 'F')),
    ket_qua VARCHAR(20) NOT NULL CHECK (ket_qua IN ('DAT', 'KHONG_DAT')),
    nhan_xet_tong_ket TEXT,
    ngay_chot_diem DATE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);


-- ==============================================================================
-- BƯỚC 3: TẠO TRIGGER TỰ ĐỘNG CẬP NHẬT updated_at CHO CÁC BẢNG
-- ==============================================================================
CREATE TRIGGER trg_tai_khoan_updated_at BEFORE UPDATE ON tai_khoan FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_sinh_vien_updated_at BEFORE UPDATE ON sinh_vien FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_giang_vien_updated_at BEFORE UPDATE ON giang_vien FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_doanh_nghiep_updated_at BEFORE UPDATE ON doanh_nghiep FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_dot_thuc_tap_updated_at BEFORE UPDATE ON dot_thuc_tap FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_phan_cong_huong_dan_updated_at BEFORE UPDATE ON phan_cong_huong_dan FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_de_cuong_thuc_tap_updated_at BEFORE UPDATE ON de_cuong_thuc_tap FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_bao_cao_dinh_ky_updated_at BEFORE UPDATE ON bao_cao_dinh_ky FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_bao_cao_tong_ket_updated_at BEFORE UPDATE ON bao_cao_tong_ket FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();
CREATE TRIGGER trg_danh_gia_ket_qua_updated_at BEFORE UPDATE ON danh_gia_ket_qua FOR EACH ROW EXECUTE PROCEDURE cap_nhat_thoi_gian_updated_at();


-- ==============================================================================
-- BƯỚC 4: TẠO INDEXES ĐỂ TỐI ƯU HÓA HIỆU NĂNG TRUY VẤN
-- ==============================================================================
CREATE INDEX idx_sinh_vien_mssv ON sinh_vien(mssv);
CREATE INDEX idx_sinh_vien_trang_thai ON sinh_vien(trang_thai);
CREATE INDEX idx_giang_vien_magv ON giang_vien(magv);
CREATE INDEX idx_giang_vien_trang_thai ON giang_vien(trang_thai);
CREATE INDEX idx_phan_cong_sv ON phan_cong_huong_dan(sinh_vien_id);
CREATE INDEX idx_phan_cong_gv ON phan_cong_huong_dan(giang_vien_id);
CREATE INDEX idx_phan_cong_dn ON phan_cong_huong_dan(doanh_nghiep_id);
CREATE INDEX idx_phan_cong_dot ON phan_cong_huong_dan(dot_thuc_tap_id);
CREATE INDEX idx_phan_cong_trang_thai ON phan_cong_huong_dan(trang_thai_duyet);
CREATE INDEX idx_bao_cao_dinh_ky_pc ON bao_cao_dinh_ky(phan_cong_id);
CREATE INDEX idx_bao_cao_tong_ket_pc ON bao_cao_tong_ket(phan_cong_id);


-- ==============================================================================
-- BƯỚC 5: CẤU HÌNH BẢO MẬT ROW LEVEL SECURITY (RLS)
-- (Tắt RLS hoặc cho phép toàn quyền truy cập để Node.js Backend & Client tương tác thuận tiện)
-- ==============================================================================
ALTER TABLE tai_khoan DISABLE ROW LEVEL SECURITY;
ALTER TABLE sinh_vien DISABLE ROW LEVEL SECURITY;
ALTER TABLE giang_vien DISABLE ROW LEVEL SECURITY;
ALTER TABLE doanh_nghiep DISABLE ROW LEVEL SECURITY;
ALTER TABLE dot_thuc_tap DISABLE ROW LEVEL SECURITY;
ALTER TABLE phan_cong_huong_dan DISABLE ROW LEVEL SECURITY;
ALTER TABLE de_cuong_thuc_tap DISABLE ROW LEVEL SECURITY;
ALTER TABLE bao_cao_dinh_ky DISABLE ROW LEVEL SECURITY;
ALTER TABLE bao_cao_tong_ket DISABLE ROW LEVEL SECURITY;
ALTER TABLE danh_gia_ket_qua DISABLE ROW LEVEL SECURITY;

-- Cấu hình Bucket Storage cho lưu trữ báo cáo PDF nếu chưa tồn tại
INSERT INTO storage.buckets (id, name, public) 
VALUES ('reports', 'reports', true)
ON CONFLICT (id) DO NOTHING;


-- ==============================================================================
-- BƯỚC 6: CHÈN DỮ LIỆU MẪU CHUẨN ĐỂ KIỂM THỬ NGAY HỆ THỐNG (SEED DATA)
-- Mật khẩu mặc định cho tất cả tài khoản là: 123456
-- ==============================================================================

-- 1. Tài khoản mẫu
INSERT INTO tai_khoan (id, ten_dang_nhap, mat_khau_hash, vai_tro, trang_thai) VALUES
(1, 'admin', '$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed', 'admin', 'hoat_dong'),
(2, 'GV_HTTT01', '$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed', 'giang_vien', 'hoat_dong'),
(3, 'HTTT2311017', '$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed', 'sinh_vien', 'hoat_dong'),
(4, 'HTTT2311020', '$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed', 'sinh_vien', 'hoat_dong');

-- Đặt lại sequence id cho bảng tai_khoan
SELECT setval('tai_khoan_id_seq', (SELECT MAX(id) FROM tai_khoan));

-- 2. Hồ sơ Giảng viên hướng dẫn
INSERT INTO giang_vien (id, tai_khoan_id, magv, hovaten, hoc_vi, bo_mon, email, sdt, so_luong_toi_da, trang_thai) VALUES
(1, 2, 'GV_HTTT01', 'ThS. Nguyễn Thúy Anh', 'Thạc sĩ', 'Bộ môn Hệ thống thông tin', 'thuyanh@ctuet.edu.vn', '0918234567', 15, 'hoat_dong');

SELECT setval('giang_vien_id_seq', (SELECT MAX(id) FROM giang_vien));

-- 3. Hồ sơ Sinh viên
INSERT INTO sinh_vien (id, tai_khoan_id, mssv, hovaten, lop, chuyen_nganh, email, sdt, gpa, trang_thai) VALUES
(1, 3, 'HTTT2311017', 'Lưu Nhật Đông', 'HTTT2311', 'Hệ thống thông tin', 'dong.httt2311017@student.ctuet.edu.vn', '0901234567', 3.65, 'hoat_dong'),
(2, 4, 'HTTT2311020', 'Trần Văn Minh', 'HTTT2311', 'Hệ thống thông tin', 'minh.httt2311020@student.ctuet.edu.vn', '0909888777', 3.42, 'hoat_dong');

SELECT setval('sinh_vien_id_seq', (SELECT MAX(id) FROM sinh_vien));

-- 4. Doanh nghiệp đối tác
INSERT INTO doanh_nghiep (id, ma_so_thue, ten_doanh_nghiep, ten_viet_tat, dia_chi, website, nguoi_lien_he, sdt_lien_he, trang_thai) VALUES
(1, '0101778163', 'Công ty Cổ phần Phần mềm FPT', 'FPT Software', 'Khu Công nghệ Cao, TP. Cần Thơ', 'https://fptsoftware.com', 'Nguyễn Văn Cường', '02923123456', 'hoat_dong'),
(2, '0300588569', 'Tập đoàn Bưu chính Viễn thông Việt Nam', 'VNPT Cần Thơ', 'Số 2 Nguyễn Thái Học, Ninh Kiều, Cần Thơ', 'https://vnpt.vn', 'Lê Thị Mai', '02923888999', 'hoat_dong');

SELECT setval('doanh_nghiep_id_seq', (SELECT MAX(id) FROM doanh_nghiep));

-- 5. Đợt thực tập
INSERT INTO dot_thuc_tap (id, ten_dot, nam_hoc, ngay_bat_dau, ngay_ket_thuc, han_dang_ky_cty, han_nop_de_cuong, trang_thai_kich_hoat) VALUES
(1, 'Thực tập tốt nghiệp HK2 (2025 - 2026)', '2025 - 2026', '2026-02-02', '2026-05-03', '2026-02-15', '2026-02-28', TRUE);

SELECT setval('dot_thuc_tap_id_seq', (SELECT MAX(id) FROM dot_thuc_tap));

-- 6. Phân công hướng dẫn
INSERT INTO phan_cong_huong_dan (id, sinh_vien_id, giang_vien_id, doanh_nghiep_id, dot_thuc_tap_id, vi_tri_thuc_tap, mentor_doanh_nghiep, sdt_mentor, loai_chu_ky_bao_cao, yeu_cau_bao_cao_giua_ky, trang_thai_duyet) VALUES
(1, 1, 1, 1, 1, 'Fullstack Developer', 'Nguyễn Văn Cường', '0988555666', 'theo_tuan', TRUE, 'dang_thuc_tap'),
(2, 2, 1, 2, 1, 'Data Analyst', 'Lê Thị Mai', '0977111222', 'theo_tuan', TRUE, 'cho_duyet');

SELECT setval('phan_cong_huong_dan_id_seq', (SELECT MAX(id) FROM phan_cong_huong_dan));

-- 7. Đề cương thực tập
INSERT INTO de_cuong_thuc_tap (id, phan_cong_id, ten_de_tai, muc_tieu, ket_qua_du_kien, cong_nghe_su_dung, noi_dung_ke_hoach, nhan_xet_gvhd, trang_thai) VALUES
(1, 1, 'Xây dựng website quản trị chuỗi bán lẻ nông sản Đồng Bằng Sông Cửu Long', 'Ứng dụng kiến trúc MVC và hệ quản trị CSDL PostgreSQL vào nghiệp vụ quản lý thực tế', 'Hệ thống web hoàn chỉnh có module phân quyền và báo cáo thống kê', 'Node.js, Express, React, PostgreSQL', '[{"tuan": 1, "cong_viec": "Khảo sát yêu cầu doanh nghiệp"}, {"tuan": 2, "cong_viec": "Thiết kế CSDL quan hệ"}]'::jsonb, 'Đề tài bám sát chuyên ngành, đồng ý kế hoạch thực tập.', 'da_duyet');

SELECT setval('de_cuong_thuc_tap_id_seq', (SELECT MAX(id) FROM de_cuong_thuc_tap));

-- 8. Nhật ký thực tập định kỳ (Báo cáo Tuần 1)
INSERT INTO bao_cao_dinh_ky (id, phan_cong_id, ky_thu, ngay_bat_dau, ngay_ket_thuc, so_ngay_lam_viec, cong_viec_hoan_thanh, kien_thuc_hoc_duoc, kho_khan_vuong_mac, diem_so, nhan_xet_gvhd, nhan_xet_mentor, ngay_nop, trang_thai) VALUES
(1, 1, 1, '2026-02-02', '2026-02-08', 5, 'Tham gia tìm hiểu tài liệu quy trình dự án, cấu hình môi trường Docker và kết nối Supabase', 'Quy trình Git Flow, Docker Compose, Supabase Client', 'Chưa quen quy chuẩn convention của nhóm dự án', 9.5, 'Thực hiện tốt các chức năng được giao, có viết tài liệu đầy đủ.', 'Sinh viên có tinh thần học hỏi cao, chấp hành tốt nội quy công ty', NOW(), 'da_cham');

SELECT setval('bao_cao_dinh_ky_id_seq', (SELECT MAX(id) FROM bao_cao_dinh_ky));

-- 9. Báo cáo tổng kết (Giữa kỳ)
INSERT INTO bao_cao_tong_ket (id, phan_cong_id, loai_bao_cao, tieu_de, tom_tat, file_bao_cao_url, link_github, diem_so, nhan_xet_gvhd, ngay_nop) VALUES
(1, 1, 'giua_ky', 'Báo cáo tiến độ thực tập giai đoạn 1', 'Đã hoàn thành 50% khối lượng công việc, xây dựng xong mô hình CSDL và luồng CRUD cơ bản', 'https://hckrmjqdvyyrzmxbunch.supabase.co/storage/v1/object/public/reports/BaoCao_GiuaKy_HTTT2311017.pdf', 'https://github.com/dong-ctuet/do-an-thuc-tap', 9.0, 'Báo cáo trình bày rõ ràng, bám sát tiến độ đề cương.', NOW());

SELECT setval('bao_cao_tong_ket_id_seq', (SELECT MAX(id) FROM bao_cao_tong_ket));

-- 10. Đánh giá kết quả (Bảng điểm mẫu kết thúc kỳ)
INSERT INTO danh_gia_ket_qua (id, phan_cong_id, diem_dinh_ky, diem_giua_ky, diem_cuoi_ky, diem_doanh_nghiep, diem_tong_ket_he10, diem_chu, ket_qua, nhan_xet_tong_ket, ngay_chot_diem) VALUES
(1, 1, 9.5, 9.0, 9.5, 9.5, 9.40, 'A+', 'DAT', 'Sinh viên thể hiện năng lực chuyên môn xuất sắc và thái độ làm việc chuyên nghiệp tại doanh nghiệp.', '2026-05-15');

SELECT setval('danh_gia_ket_qua_id_seq', (SELECT MAX(id) FROM danh_gia_ket_qua));
