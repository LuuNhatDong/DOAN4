-- =========================================================================
-- ĐỒ ÁN 4: HỆ THỐNG QUẢN LÝ THỰC TẬP DOANH NGHIỆP - KHOA CÔNG NGHỆ THÔNG TIN
-- Script: Cập nhật 9 Giảng viên BM HTTT & Nạp 57 Sinh viên 4 ngành Khoa CNTT
-- =========================================================================

-- 1. CẬP NHẬT HOẶC BỔ SUNG 9 GIẢNG VIÊN BỘ MÔN HTTT (KHOA CNTT - CTUET)
DO $$
DECLARE
    gv_record RECORD;
    v_tk_id BIGINT;
    v_pass_hash TEXT := '$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed';
BEGIN
    FOR gv_record IN
        SELECT * FROM (VALUES
            ('GV_HTTT01', 'TS. Nguyễn Thị Hồng Hạnh', 'Tiến sĩ', 'Hệ thống thông tin', 'nthhanh@ctuet.edu.vn', '0901111001', 15),
            ('GV_HTTT02', 'NCS. Nguyễn Trung Việt', 'NCS. Thạc sĩ', 'Hệ thống thông tin', 'ntviet@ctuet.edu.vn', '0901111002', 15),
            ('GV_HTTT03', 'ThS. Nguyễn Văn Cường', 'Thạc sĩ', 'Hệ thống thông tin', 'nvcuong@ctuet.edu.vn', '0901111003', 12),
            ('GV_HTTT04', 'ThS. Phạm Yến Nhi', 'Thạc sĩ', 'Hệ thống thông tin', 'pynhi@ctuet.edu.vn', '0901111004', 12),
            ('GV_HTTT05', 'ThS. Trần Thị Thùy Dương', 'Thạc sĩ', 'Hệ thống thông tin', 'tttduong@ctuet.edu.vn', '0901111005', 12),
            ('GV_HTTT06', 'ThS. Nguyễn Tấn Phú', 'Thạc sĩ', 'Hệ thống thông tin', 'ntphu@ctuet.edu.vn', '0901111006', 12),
            ('GV_HTTT07', 'ThS. Nguyễn Phan Tú', 'Thạc sĩ', 'Hệ thống thông tin', 'nptu@ctuet.edu.vn', '0901111007', 10),
            ('GV_HTTT08', 'TS. Tạ Thanh Thủy Tiên', 'Tiến sĩ', 'Hệ thống thông tin', 'ttttien@ctuet.edu.vn', '0901111008', 10),
            ('GV_HTTT09', 'ThS. Nguyễn Thị Ngọc Như', 'Thạc sĩ', 'Hệ thống thông tin', 'ntnnhu@ctuet.edu.vn', '0901111009', 10)
        ) AS t(magv, hovaten, hoc_vi, bo_mon, email, sdt, so_luong_toi_da)
    LOOP
        -- Kiểm tra tài khoản đã tồn tại chưa
        SELECT id INTO v_tk_id FROM public.tai_khoan WHERE ten_dang_nhap = gv_record.magv;
        IF v_tk_id IS NULL THEN
            INSERT INTO public.tai_khoan (ten_dang_nhap, mat_khau_hash, vai_tro, trang_thai)
            VALUES (gv_record.magv, v_pass_hash, 'giang_vien', 'hoat_dong')
            RETURNING id INTO v_tk_id;
        END IF;

        -- Thêm hoặc cập nhật giảng viên
        INSERT INTO public.giang_vien (tai_khoan_id, magv, hovaten, hoc_vi, bo_mon, email, sdt, so_luong_toi_da, trang_thai)
        VALUES (v_tk_id, gv_record.magv, gv_record.hovaten, gv_record.hoc_vi, gv_record.bo_mon, gv_record.email, gv_record.sdt, gv_record.so_luong_toi_da, 'hoat_dong')
        ON CONFLICT (magv) DO UPDATE
        SET hovaten = EXCLUDED.hovaten,
            hoc_vi = EXCLUDED.hoc_vi,
            bo_mon = EXCLUDED.bo_mon,
            email = EXCLUDED.email,
            sdt = EXCLUDED.sdt,
            so_luong_toi_da = EXCLUDED.so_luong_toi_da,
            trang_thai = 'hoat_dong',
            updated_at = NOW();
    END LOOP;
END $$;

-- 2. NẠP 57 SINH VIÊN 4 NGÀNH (HTTT, CNTT, KHMT, KTPM)
DO $$
DECLARE
    sv_record RECORD;
    v_tk_id BIGINT;
    v_sv_id BIGINT;
    v_dot_id BIGINT;
    v_gv_id BIGINT;
    v_pass_hash TEXT := '$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed';
BEGIN
    -- Lấy đợt thực tập đang kích hoạt
    SELECT id INTO v_dot_id FROM public.dot_thuc_tap WHERE trang_thai_kich_hoat = TRUE LIMIT 1;
    IF v_dot_id IS NULL THEN
        SELECT id INTO v_dot_id FROM public.dot_thuc_tap ORDER BY id DESC LIMIT 1;
    END IF;

    -- Lấy ID giảng viên trưởng bộ môn làm mặc định mẫu cho một số sinh viên
    SELECT id INTO v_gv_id FROM public.giang_vien WHERE magv = 'GV_HTTT01' LIMIT 1;

    FOR sv_record IN
        SELECT * FROM (VALUES
            -- 1. Ngành Hệ thống thông tin (15 Sinh viên)
            ('HTTT2311017', 'Lưu Nhật Đông', 'HTTT2311', 'Hệ thống thông tin', 'lndonghttt2311017@student.ctuet.edu.vn', '0901234567', 3.65),
            ('HTTT2311033', 'Nguyễn Lâm Quang Hà', 'HTTT2311', 'Hệ thống thông tin', 'nlqhahttt2311033@student.ctuet.edu.vn', '0901234568', 3.42),
            ('HTTT2311040', 'Phạm Hoàng Thiện', 'HTTT2311', 'Hệ thống thông tin', 'phthienhttt2311040@student.ctuet.edu.vn', '0901234569', 3.30),
            ('HTTT2311014', 'Bùi Thành Long', 'HTTT2311', 'Hệ thống thông tin', 'btlonghttt2311014@student.ctuet.edu.vn', '0901234570', 3.55),
            ('HTTT2311032', 'Liễu Hiếu Nhi', 'HTTT2311', 'Hệ thống thông tin', 'lhnhihttt2311032@student.ctuet.edu.vn', '0901234571', 3.70),
            ('HTTT2311013', 'Nguyễn Trần An Khang', 'HTTT2311', 'Hệ thống thông tin', 'ntakhanghttt2311013@student.ctuet.edu.vn', '0901234572', 3.25),
            ('HTTT2311047', 'Đinh Thị Thu Cúc', 'HTTT2311', 'Hệ thống thông tin', 'dttcuchttt2311047@student.ctuet.edu.vn', '0901234573', 3.48),
            ('HTTT2311024', 'Nguyễn Vũ Khang', 'HTTT2311', 'Hệ thống thông tin', 'nvkhanghttt2311024@student.ctuet.edu.vn', '0901234574', 3.35),
            ('HTTT2311022', 'Nguyễn Thị Thùy Trang', 'HTTT2311', 'Hệ thống thông tin', 'ntttranghttt2311022@student.ctuet.edu.vn', '0901234575', 3.60),
            ('HTTT2311008', 'Lê Hồ Quang Thông', 'HTTT2311', 'Hệ thống thông tin', 'lhqthonghttt2311008@student.ctuet.edu.vn', '0901234576', 3.15),
            ('HTTT2311049', 'Nguyễn Ngọc Nhi', 'HTTT2311', 'Hệ thống thông tin', 'nnnhihttt2311049@student.ctuet.edu.vn', '0901234577', 3.52),
            ('HTTT2311036', 'Nguyễn Hữu Hào', 'HTTT2311', 'Hệ thống thông tin', 'nhhaohttt2311036@student.ctuet.edu.vn', '0901234578', 3.20),
            ('HTTT2311029', 'Võ Quốc Vinh', 'HTTT2311', 'Hệ thống thông tin', 'vqvinhhttt2311029@student.ctuet.edu.vn', '0901234579', 3.40),
            ('HTTT2311037', 'Nguyễn Trần Duy Khang', 'HTTT2311', 'Hệ thống thông tin', 'ntdkhanghttt2311037@student.ctuet.edu.vn', '0901234580', 3.18),
            ('HTTT2311058', 'Huỳnh Thị Bảo Hân', 'HTTT2311', 'Hệ thống thông tin', 'htbhanhttt2311058@student.ctuet.edu.vn', '0901234581', 3.58),

            -- 2. Ngành Công nghệ thông tin (14 Sinh viên)
            ('CNTT2311052', 'Nguyễn Minh Anh Tuấn', 'CNTT2311', 'Công nghệ thông tin', 'nmatuancntt2311052@student.ctuet.edu.vn', '0912345601', 3.32),
            ('CNTT2311043', 'Nguyễn Anh Kiệt', 'CNTT2311', 'Công nghệ thông tin', 'nakietcntt2311043@student.ctuet.edu.vn', '0912345602', 3.45),
            ('CNTT2311053', 'Trần Thị Ngọc Mỹ', 'CNTT2311', 'Công nghệ thông tin', 'ttnmycntt2311053@student.ctuet.edu.vn', '0912345603', 3.61),
            ('CNTT2311010', 'Hồ Minh Thiện', 'CNTT2311', 'Công nghệ thông tin', 'hmthiencntt2311010@student.ctuet.edu.vn', '0912345604', 3.28),
            ('CNTT2311019', 'Quách Thành Danh', 'CNTT2311', 'Công nghệ thông tin', 'qtdanhcntt2311019@student.ctuet.edu.vn', '0912345605', 3.50),
            ('CNTT2311054', 'Đỗ Quốc Đạt', 'CNTT2311', 'Công nghệ thông tin', 'dqdatcntt2311054@student.ctuet.edu.vn', '0912345606', 3.19),
            ('CNTT2311007', 'Ngô Thị Anh Thư', 'CNTT2311', 'Công nghệ thông tin', 'ntathucntt2311007@student.ctuet.edu.vn', '0912345607', 3.64),
            ('CNTT2311038', 'Trần Thanh Hương', 'CNTT2311', 'Công nghệ thông tin', 'tthuongcntt2311038@student.ctuet.edu.vn', '0912345608', 3.38),
            ('CNTT2311055', 'Nguyễn Trung Hậu', 'CNTT2311', 'Công nghệ thông tin', 'nthaucntt2311055@student.ctuet.edu.vn', '0912345609', 3.22),
            ('CNTT2311026', 'Đỗ Nguyễn Minh Thư', 'CNTT2311', 'Công nghệ thông tin', 'dnmthucntt2311026@student.ctuet.edu.vn', '0912345610', 3.72),
            ('CNTT2311016', 'Doan Thanh Long', 'CNTT2311', 'Công nghệ thông tin', 'dtlongcntt2311016@student.ctuet.edu.vn', '0912345611', 3.10),
            ('CNTT2311051', 'Trần Quỳnh Mai', 'CNTT2311', 'Công nghệ thông tin', 'tqmaicntt2311051@student.ctuet.edu.vn', '0912345612', 3.56),
            ('CNTT2311046', 'Trần Thiên Phú', 'CNTT2311', 'Công nghệ thông tin', 'ttphucntt2311046@student.ctuet.edu.vn', '0912345613', 3.41),
            ('CNTT2311018', 'Phan Đặng Đức Nguyên', 'CNTT2311', 'Công nghệ thông tin', 'pddnguyencntt2311018@student.ctuet.edu.vn', '0912345614', 3.29),

            -- 3. Ngành Khoa học máy tính (14 Sinh viên)
            ('KHMT2311042', 'Huỳnh Nguyên Toàn', 'KHMT2311', 'Khoa học máy tính', 'hntoankhmt2311042@student.ctuet.edu.vn', '0987654301', 3.54),
            ('KHMT2311048', 'Nguyễn Thị Cẩm Tiên', 'KHMT2311', 'Khoa học máy tính', 'ntctienkhmt2311048@student.ctuet.edu.vn', '0987654302', 3.68),
            ('KHMT2311057', 'Phạm Thúy Huỳnh', 'KHMT2311', 'Khoa học máy tính', 'pthuynhkhmt2311057@student.ctuet.edu.vn', '0987654303', 3.36),
            ('KHMT2311011', 'Phan Trần Minh Khuê', 'KHMT2311', 'Khoa học máy tính', 'ptmkhuekhmt2311011@student.ctuet.edu.vn', '0987654304', 3.47),
            ('KHMT2311020', 'NGUYỄN ĐẬU TUỆ KHƯƠNG', 'KHMT2311', 'Khoa học máy tính', 'ndtkhuongkhmt2311020@student.ctuet.edu.vn', '0987654305', 3.82),
            ('KHMT2311021', 'Huỳnh Nguyễn Xuân Thi', 'KHMT2311', 'Khoa học máy tính', 'hnxthikhmt2311021@student.ctuet.edu.vn', '0987654306', 3.25),
            ('KHMT2311045', 'Trần Quốc Hùng', 'KHMT2311', 'Khoa học máy tính', 'tqhungkhmt2311045@student.ctuet.edu.vn', '0987654307', 3.15),
            ('KHMT2311060', 'Võ Hoàng Nhã', 'KHMT2311', 'Khoa học máy tính', 'vnhakhmt2311060@student.ctuet.edu.vn', '0987654308', 3.44),
            ('KHMT2311066', 'Hoàng Thị Ngọc Mai', 'KHMT2311', 'Khoa học máy tính', 'htnmaikhmt2311066@student.ctuet.edu.vn', '0987654309', 3.75),
            ('KHMT2311056', 'PHẠM VĂN TẦN PHƯỚC', 'KHMT2311', 'Khoa học máy tính', 'pvtphuockhmt2311056@student.ctuet.edu.vn', '0987654310', 3.20),
            ('KHMT2311039', 'Nguyễn Đắc Nhân', 'KHMT2311', 'Khoa học máy tính', 'ndnhankhmt2311039@student.ctuet.edu.vn', '0987654311', 3.39),
            ('KHMT2311009', 'Huỳnh Gia Tuấn', 'KHMT2311', 'Khoa học máy tính', 'hgtuankhmt2311009@student.ctuet.edu.vn', '0987654312', 3.51),
            ('KHMT2311023', 'Bùi Hữu Lộc', 'KHMT2311', 'Khoa học máy tính', 'bhlockhmt2311023@student.ctuet.edu.vn', '0987654313', 3.30),
            ('KHMT2311015', 'Hồ Trần Phương Anh', 'KHMT2311', 'Khoa học máy tính', 'htpanhkhmt2311015@student.ctuet.edu.vn', '0987654314', 3.62),

            -- 4. Ngành Kỹ thuật phần mềm (14 Sinh viên)
            ('KTPM2311004', 'Nguyễn Đức Lương', 'KTPM2311', 'Kỹ thuật phần mềm', 'ndluongktpm2311004@student.ctuet.edu.vn', '0934567801', 3.58),
            ('KTPM2311061', 'Nguyễn Ngọc Tường Vy', 'KTPM2311', 'Kỹ thuật phần mềm', 'nntvyktpm2311061@student.ctuet.edu.vn', '0934567802', 3.65),
            ('KTPM2311030', 'Phan Thiện Nhân', 'KTPM2311', 'Kỹ thuật phần mềm', 'ptnhanktpm2311030@student.ctuet.edu.vn', '0934567803', 3.28),
            ('KTPM2311062', 'Lý Minh Lộc', 'KTPM2311', 'Kỹ thuật phần mềm', 'lmlocktpm2311062@student.ctuet.edu.vn', '0934567804', 3.42),
            ('KTPM2311064', 'Trần Ngọc Ản', 'KTPM2311', 'Kỹ thuật phần mềm', 'tnanckpm2311064@student.ctuet.edu.vn', '0934567805', 3.35),
            ('KTPM2311006', 'Bùi Diệp Ngọc Hân', 'KTPM2311', 'Kỹ thuật phần mềm', 'bdnhanktpm2311006@student.ctuet.edu.vn', '0934567806', 3.70),
            ('KTPM2311012', 'Trần Thị Huyền Trân', 'KTPM2311', 'Kỹ thuật phần mềm', 'tthtranktpm2311012@student.ctuet.edu.vn', '0934567807', 3.49),
            ('KTPM2311025', 'Trần Toàn Phát', 'KTPM2311', 'Kỹ thuật phần mềm', 'ttphatktpm2311025@student.ctuet.edu.vn', '0934567808', 3.21),
            ('KTPM2311063', 'Kha Minh Khang', 'KTPM2311', 'Kỹ thuật phần mềm', 'kmkhangktpm2311063@student.ctuet.edu.vn', '0934567809', 3.53),
            ('KTPM2311041', 'Đặng Khánh Hoà', 'KTPM2311', 'Kỹ thuật phần mềm', 'dkhoaktpm2311041@student.ctuet.edu.vn', '0934567810', 3.37),
            ('KTPM2311069', 'Lê Nhật Trường', 'KTPM2311', 'Kỹ thuật phần mềm', 'lntruongktpm2311069@student.ctuet.edu.vn', '0934567811', 3.18),
            ('KTPM2311065', 'Nguyễn Vũ Hà', 'KTPM2311', 'Kỹ thuật phần mềm', 'nvhaktpm2311065@student.ctuet.edu.vn', '0934567812', 3.40),
            ('KTPM2311067', 'Phạm Thị Khánh Vy', 'KTPM2311', 'Kỹ thuật phần mềm', 'ptkvyktpm2311067@student.ctuet.edu.vn', '0934567813', 3.63),
            ('KTPM2311070', 'Lê Nhật Trường (2)', 'KTPM2311', 'Kỹ thuật phần mềm', 'lntruongktpm2311070@student.ctuet.edu.vn', '0934567814', 3.12)
        ) AS t(mssv, hovaten, lop, chuyen_nganh, email, sdt, gpa)
    LOOP
        -- 1. Tạo tài khoản nếu chưa có
        SELECT id INTO v_tk_id FROM public.tai_khoan WHERE ten_dang_nhap = sv_record.mssv;
        IF v_tk_id IS NULL THEN
            INSERT INTO public.tai_khoan (ten_dang_nhap, mat_khau_hash, vai_tro, trang_thai)
            VALUES (sv_record.mssv, v_pass_hash, 'sinh_vien', 'hoat_dong')
            RETURNING id INTO v_tk_id;
        END IF;

        -- 2. Thêm hoặc cập nhật sinh viên
        INSERT INTO public.sinh_vien (tai_khoan_id, mssv, hovaten, lop, chuyen_nganh, email, sdt, gpa, trang_thai)
        VALUES (v_tk_id, sv_record.mssv, sv_record.hovaten, sv_record.lop, sv_record.chuyen_nganh, sv_record.email, sv_record.sdt, sv_record.gpa, 'hoat_dong')
        ON CONFLICT (mssv) DO UPDATE
        SET hovaten = EXCLUDED.hovaten,
            lop = EXCLUDED.lop,
            chuyen_nganh = EXCLUDED.chuyen_nganh,
            email = EXCLUDED.email,
            sdt = EXCLUDED.sdt,
            gpa = EXCLUDED.gpa,
            trang_thai = 'hoat_dong',
            updated_at = NOW()
        RETURNING id INTO v_sv_id;

        -- 3. Tạo phân công trong đợt thực tập nếu chưa có
        IF v_dot_id IS NOT NULL AND v_sv_id IS NOT NULL THEN
            IF NOT EXISTS (SELECT 1 FROM public.phan_cong_huong_dan WHERE sinh_vien_id = v_sv_id AND dot_thuc_tap_id = v_dot_id) THEN
                -- Với SV đầu tiên (Lưu Nhật Đông), phân công cho TS. Nguyễn Thị Hồng Hạnh
                IF sv_record.mssv = 'HTTT2311017' THEN
                    INSERT INTO public.phan_cong_huong_dan (
                        sinh_vien_id, dot_thuc_tap_id, giang_vien_id, ten_cty_ngoai, vi_tri_thuc_tap, 
                        mentor_doanh_nghiep, sdt_mentor, loai_chu_ky_bao_cao, yeu_cau_bao_cao_giua_ky, trang_thai_duyet
                    ) VALUES (
                        v_sv_id, v_dot_id, v_gv_id, 'Công ty Cổ phần Phần mềm FPT Cần Thơ', 
                        'Lập trình viên .NET / C# Backend', 'Trần Văn Hưng (Tech Lead)', '0918112233', 'theo_thang', TRUE, 'dang_thuc_tap'
                    );
                -- Với SV thứ hai (Nguyễn Lâm Quang Hà), phân công theo tuần
                ELSIF sv_record.mssv = 'HTTT2311033' THEN
                    INSERT INTO public.phan_cong_huong_dan (
                        sinh_vien_id, dot_thuc_tap_id, giang_vien_id, ten_cty_ngoai, vi_tri_thuc_tap, 
                        mentor_doanh_nghiep, sdt_mentor, loai_chu_ky_bao_cao, yeu_cau_bao_cao_giua_ky, trang_thai_duyet
                    ) VALUES (
                        v_sv_id, v_dot_id, v_gv_id, 'Tập đoàn Viettel - Chi nhánh Cần Thơ', 
                        'Chuyên viên Phân tích nghiệp vụ BA', 'Nguyễn Thị Loan (Trưởng phòng)', '0988776655', 'theo_tuan', TRUE, 'dang_thuc_tap'
                    );
                ELSE
                    -- Các SV khác tạo trạng thái chờ duyệt hoặc tự do khai báo
                    INSERT INTO public.phan_cong_huong_dan (
                        sinh_vien_id, dot_thuc_tap_id, giang_vien_id, ten_cty_ngoai, vi_tri_thuc_tap, loai_chu_ky_bao_cao, trang_thai_duyet
                    ) VALUES (
                        v_sv_id, v_dot_id, NULL, 'Doanh nghiệp CNTT thực tập', 'Thực tập sinh IT', 'theo_tuan', 'cho_duyet'
                    );
                END IF;
            END IF;
        END IF;
    END LOOP;
END $$;
