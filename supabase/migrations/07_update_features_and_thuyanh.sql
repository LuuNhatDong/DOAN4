-- =========================================================================
-- ĐỒ ÁN 4: HỆ THỐNG QUẢN LÝ THỰC TẬP DOANH NGHIỆP - KHOA CÔNG NGHỆ THÔNG TIN
-- Script: Cập nhật CSDL, Thêm ThS. Nguyễn Thúy Anh và Nạp dữ liệu mẫu phong phú
-- =========================================================================

-- 1. Bổ sung các cột mới và mở rộng ràng buộc trạng thái vào bảng phan_cong_huong_dan
ALTER TABLE public.phan_cong_huong_dan 
ADD COLUMN IF NOT EXISTS so_thang_thuc_tap INT DEFAULT 3,
ADD COLUMN IF NOT EXISTS ly_do_tu_choi TEXT;

ALTER TABLE public.phan_cong_huong_dan 
DROP CONSTRAINT IF EXISTS phan_cong_huong_dan_trang_thai_duyet_check;

ALTER TABLE public.phan_cong_huong_dan 
ADD CONSTRAINT phan_cong_huong_dan_trang_thai_duyet_check 
CHECK (trang_thai_duyet IN ('cho_duyet', 'cho_gv_duyet', 'gv_tu_choi', 'da_duyet', 'tu_choi', 'dang_thuc_tap', 'hoan_thanh'));

-- 2. Thêm Giảng viên ThS. Nguyễn Thúy Anh
DO $$
DECLARE
    v_tk_id BIGINT;
    v_gv_id BIGINT;
    v_dot_id BIGINT;
    v_sv1_id BIGINT;
    v_sv2_id BIGINT;
    v_sv3_id BIGINT;
    v_sv4_id BIGINT;
    v_pc1_id BIGINT;
    v_pc2_id BIGINT;
    v_pass_hash TEXT := '$2b$10$w8uTqN82nQh92E4z524ZMe5JvXw0r0rFq6w3W8.123456hashed';
BEGIN
    -- Lấy hoặc tạo tài khoản cho ThS. Nguyễn Thúy Anh
    SELECT id INTO v_tk_id FROM public.tai_khoan WHERE ten_dang_nhap = 'GV_THUYANH';
    IF v_tk_id IS NULL THEN
        INSERT INTO public.tai_khoan (ten_dang_nhap, mat_khau_hash, vai_tro, trang_thai)
        VALUES ('GV_THUYANH', v_pass_hash, 'giang_vien', 'hoat_dong')
        RETURNING id INTO v_tk_id;
    END IF;

    -- Thêm hoặc cập nhật hồ sơ giảng viên
    INSERT INTO public.giang_vien (tai_khoan_id, magv, hovaten, hoc_vi, bo_mon, email, sdt, so_luong_toi_da, trang_thai)
    VALUES (v_tk_id, 'GV_THUYANH', 'ThS. Nguyễn Thúy Anh', 'Thạc sĩ', 'Hệ thống thông tin', 'ntanh@ctuet.edu.vn', '0901111999', 12, 'hoat_dong')
    ON CONFLICT (magv) DO UPDATE
    SET hovaten = EXCLUDED.hovaten,
        hoc_vi = EXCLUDED.hoc_vi,
        email = EXCLUDED.email,
        sdt = EXCLUDED.sdt,
        so_luong_toi_da = EXCLUDED.so_luong_toi_da,
        trang_thai = 'hoat_dong'
    RETURNING id INTO v_gv_id;

    -- Lấy đợt thực tập đang kích hoạt
    SELECT id INTO v_dot_id FROM public.dot_thuc_tap WHERE trang_thai_kich_hoat = TRUE LIMIT 1;
    IF v_dot_id IS NULL THEN
        SELECT id INTO v_dot_id FROM public.dot_thuc_tap ORDER BY id DESC LIMIT 1;
    END IF;

    -- Lấy ID các sinh viên mẫu
    SELECT id INTO v_sv1_id FROM public.sinh_vien WHERE mssv = 'HTTT2311017';
    SELECT id INTO v_sv2_id FROM public.sinh_vien WHERE mssv = 'HTTT2311033';
    SELECT id INTO v_sv3_id FROM public.sinh_vien WHERE mssv = 'CNTT2311052';
    SELECT id INTO v_sv4_id FROM public.sinh_vien WHERE mssv = 'KHMT2311042';

    -- 3. Cấu hình SV 1 (Lưu Nhật Đông): Thực tập 3 tháng, báo cáo theo tháng, GVHD là ThS. Nguyễn Thúy Anh
    IF v_sv1_id IS NOT NULL AND v_dot_id IS NOT NULL THEN
        UPDATE public.phan_cong_huong_dan
        SET giang_vien_id = v_gv_id,
            ten_cty_ngoai = 'Công ty Cổ phần Phần mềm FPT Cần Thơ',
            vi_tri_thuc_tap = 'Lập trình viên .NET / C# Backend',
            so_thang_thuc_tap = 3,
            loai_chu_ky_bao_cao = 'theo_thang',
            yeu_cau_bao_cao_giua_ky = TRUE,
            trang_thai_duyet = 'dang_thuc_tap',
            updated_at = NOW()
        WHERE sinh_vien_id = v_sv1_id AND dot_thuc_tap_id = v_dot_id
        RETURNING id INTO v_pc1_id;

        IF v_pc1_id IS NOT NULL THEN
            -- Xóa đề cương cũ nếu có rồi nạp mới
            DELETE FROM public.de_cuong_thuc_tap WHERE phan_cong_id = v_pc1_id;

            INSERT INTO public.de_cuong_thuc_tap (phan_cong_id, ten_de_tai, muc_tieu, ket_qua_du_kien, cong_nghe_su_dung, noi_dung_ke_hoach, trang_thai, nhan_xet_gvhd)
            VALUES (
                v_pc1_id, 
                'Xây dựng hệ thống quản lý chuỗi bán lẻ nông sản công nghệ cao',
                'Nghiên cứu kiến trúc Microservices và lập trình C# ASP.NET Core trong doanh nghiệp',
                'Phần mềm Web quản trị, API dịch vụ và tài liệu thiết kế hệ thống',
                'C#, ASP.NET Core, PostgreSQL, Bootstrap 5, Docker',
                '[{"giai_doan": 1, "cong_viec": "Tiếp nhận và khảo sát quy trình vận hành"}, {"giai_doan": 2, "cong_viec": "Thiết kế CSDL quan hệ PostgreSQL"}, {"giai_doan": 3, "cong_viec": "Phát triển các module chức năng và kiểm thử"}, {"giai_doan": 4, "cong_viec": "Bàn giao và tổng kết kết quả"}]'::jsonb,
                'da_duyet',
                'Đề tài có tính ứng dụng thực tiễn cao, đề cương chi tiết rõ ràng, GVHD phê duyệt thực hiện.'
            );

            -- Xóa báo cáo cũ nếu có để nạp lại mẫu chuẩn
            DELETE FROM public.bao_cao_dinh_ky WHERE phan_cong_id = v_pc1_id;

            -- Tháng 1: Đã chấm 9.0
            INSERT INTO public.bao_cao_dinh_ky (phan_cong_id, ky_thu, ngay_bat_dau, ngay_ket_thuc, so_ngay_lam_viec, cong_viec_hoan_thanh, kien_thuc_hoc_duoc, kho_khan_vuong_mac, diem_so, trang_thai, nhan_xet_gvhd)
            VALUES (
                v_pc1_id, 1, 
                CURRENT_DATE - INTERVAL '60 days', CURRENT_DATE - INTERVAL '31 days', 22,
                'Khảo sát quy trình vận hành chuỗi cửa hàng, phân tích yêu cầu nghiệp vụ và thiết kế cơ sở dữ liệu PostgreSQL',
                'Nắm vững quy trình nghiệp vụ bán lẻ thực tế, kỹ năng làm việc nhóm Agile/Scrum',
                'Thời gian đầu làm quen với cơ sở dữ liệu lớn còn bỡ ngỡ nhưng đã giải quyết tốt',
                9.0, 'da_cham',
                'Sinh viên nắm bắt công việc nhanh, hoàn thành tốt mục tiêu đề ra cho tháng đầu tiên.'
            );

            -- Tháng 2: Đã nộp, chờ chấm
            INSERT INTO public.bao_cao_dinh_ky (phan_cong_id, ky_thu, ngay_bat_dau, ngay_ket_thuc, so_ngay_lam_viec, cong_viec_hoan_thanh, kien_thuc_hoc_duoc, kho_khan_vuong_mac, diem_so, trang_thai, nhan_xet_gvhd)
            VALUES (
                v_pc1_id, 2, 
                CURRENT_DATE - INTERVAL '30 days', CURRENT_DATE - INTERVAL '1 day', 22,
                'Lập trình xây dựng module Quản lý kho hàng và API xử lý đơn đặt hàng trực tuyến',
                'Thành thạo Entity Framework Core, LINQ, Repository Pattern và kiểm thử đơn vị Unit Test',
                'Xử lý đồng thời (concurrency) khi nhiều đơn hàng đặt cùng lúc',
                NULL, 'da_nop', NULL
            );
        END IF;
    END IF;

    -- 4. Cấu hình SV 2 (Nguyễn Lâm Quang Hà): Thực tập 2 tháng, báo cáo theo tuần (8 tuần), GVHD là ThS. Nguyễn Thúy Anh
    IF v_sv2_id IS NOT NULL AND v_dot_id IS NOT NULL THEN
        UPDATE public.phan_cong_huong_dan
        SET giang_vien_id = v_gv_id,
            ten_cty_ngoai = 'Tập đoàn Viettel - Chi nhánh Cần Thơ',
            vi_tri_thuc_tap = 'Chuyên viên Phân tích nghiệp vụ BA',
            so_thang_thuc_tap = 2,
            loai_chu_ky_bao_cao = 'theo_tuan',
            yeu_cau_bao_cao_giua_ky = TRUE,
            trang_thai_duyet = 'dang_thuc_tap',
            updated_at = NOW()
        WHERE sinh_vien_id = v_sv2_id AND dot_thuc_tap_id = v_dot_id
        RETURNING id INTO v_pc2_id;

        IF v_pc2_id IS NOT NULL THEN
            DELETE FROM public.de_cuong_thuc_tap WHERE phan_cong_id = v_pc2_id;

            INSERT INTO public.de_cuong_thuc_tap (phan_cong_id, ten_de_tai, muc_tieu, ket_qua_du_kien, cong_nghe_su_dung, noi_dung_ke_hoach, trang_thai, nhan_xet_gvhd)
            VALUES (
                v_pc2_id, 
                'Phân tích nghiệp vụ và tối ưu hóa quy trình chăm sóc khách hàng viễn thông',
                'Nâng cao kỹ năng phân tích đặc tả yêu cầu SRS và biểu đồ BPMN',
                'Bộ tài liệu đặc tả SRS, tài liệu hướng dẫn người dùng và quy trình nghiệp vụ mới',
                'BPMN, UML, Draw.io, Jira, Figma',
                '[{"tuan": 1, "cong_viec": "Khảo sát hiện trạng quy trình CSKH tại chi nhánh"}, {"tuan": 2, "cong_viec": "Phỏng vấn các phòng ban và thu thập yêu cầu người dùng"}, {"tuan": 3, "cong_viec": "Vẽ biểu đồ luồng nghiệp vụ hiện tại AS-IS và đề xuất TO-BE"}, {"tuan": 4, "cong_viec": "Hoàn thiện tài liệu đặc tả yêu cầu phần mềm SRS v1.0"}]'::jsonb,
                'da_duyet',
                'Kế hoạch rất chặt chẽ, duyệt thực hiện.'
            );

            DELETE FROM public.bao_cao_dinh_ky WHERE phan_cong_id = v_pc2_id;

            -- Tuần 1, 2, 3 đã chấm
            INSERT INTO public.bao_cao_dinh_ky (phan_cong_id, ky_thu, ngay_bat_dau, ngay_ket_thuc, so_ngay_lam_viec, cong_viec_hoan_thanh, kien_thuc_hoc_duoc, diem_so, trang_thai, nhan_xet_gvhd)
            VALUES 
            (v_pc2_id, 1, CURRENT_DATE - INTERVAL '28 days', CURRENT_DATE - INTERVAL '22 days', 5, 'Khảo sát hiện trạng quy trình CSKH tại chi nhánh', 'Làm quen văn hóa làm việc và quy định Viettel', 8.5, 'da_cham', 'Bắt nhịp tốt.'),
            (v_pc2_id, 2, CURRENT_DATE - INTERVAL '21 days', CURRENT_DATE - INTERVAL '15 days', 5, 'Phỏng vấn các phòng ban và thu thập yêu cầu người dùng', 'Kỹ năng phỏng vấn thu thập yêu cầu chuyên nghiệp', 9.0, 'da_cham', 'Báo cáo rất chi tiết.'),
            (v_pc2_id, 3, CURRENT_DATE - INTERVAL '14 days', CURRENT_DATE - INTERVAL '8 days', 5, 'Vẽ biểu đồ luồng nghiệp vụ hiện tại AS-IS và đề xuất TO-BE', 'Sử dụng BPMN chuẩn quốc tế', 9.0, 'da_cham', 'Rất tiến bộ.');

            -- Tuần 4 chờ chấm
            INSERT INTO public.bao_cao_dinh_ky (phan_cong_id, ky_thu, ngay_bat_dau, ngay_ket_thuc, so_ngay_lam_viec, cong_viec_hoan_thanh, kien_thuc_hoc_duoc, diem_so, trang_thai, nhan_xet_gvhd)
            VALUES (v_pc2_id, 4, CURRENT_DATE - INTERVAL '7 days', CURRENT_DATE - INTERVAL '1 day', 5, 'Hoàn thiện tài liệu đặc tả yêu cầu phần mềm SRS v1.0', 'Kỹ năng viết tài liệu kỹ thuật', NULL, 'da_nop', NULL);
        END IF;
    END IF;

    -- 5. SV 3 (Nguyễn Minh Anh Tuấn - CNTT): Gửi yêu cầu hướng dẫn đến ThS. Nguyễn Thúy Anh (cho_gv_duyet)
    IF v_sv3_id IS NOT NULL AND v_dot_id IS NOT NULL THEN
        UPDATE public.phan_cong_huong_dan
        SET giang_vien_id = v_gv_id,
            ten_cty_ngoai = 'Công ty TNHH Giải Pháp Phần Mềm VNPT Cần Thơ',
            vi_tri_thuc_tap = 'Thực tập sinh Lập trình Fullstack',
            so_thang_thuc_tap = 3,
            loai_chu_ky_bao_cao = 'theo_thang',
            trang_thai_duyet = 'cho_gv_duyet',
            updated_at = NOW()
        WHERE sinh_vien_id = v_sv3_id AND dot_thuc_tap_id = v_dot_id;
    END IF;

    -- 6. SV 4 (Huỳnh Nguyên Toàn - KHMT): Gửi yêu cầu hướng dẫn đến ThS. Nguyễn Thúy Anh (cho_gv_duyet)
    IF v_sv4_id IS NOT NULL AND v_dot_id IS NOT NULL THEN
        UPDATE public.phan_cong_huong_dan
        SET giang_vien_id = v_gv_id,
            ten_cty_ngoai = 'Trung tâm Đổi mới Sáng tạo & Trí tuệ Nhân tạo',
            vi_tri_thuc_tap = 'Thực tập sinh Phân tích dữ liệu & AI',
            so_thang_thuc_tap = 2,
            loai_chu_ky_bao_cao = 'theo_thang',
            trang_thai_duyet = 'cho_gv_duyet',
            updated_at = NOW()
        WHERE sinh_vien_id = v_sv4_id AND dot_thuc_tap_id = v_dot_id;
    END IF;

END $$;
