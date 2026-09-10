-- Row Level Security (RLS) Policies

-- Enable RLS on all tables
ALTER TABLE profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE companies ENABLE ROW LEVEL SECURITY;
ALTER TABLE lecturers ENABLE ROW LEVEL SECURITY;
ALTER TABLE students ENABLE ROW LEVEL SECURITY;
ALTER TABLE internship_periods ENABLE ROW LEVEL SECURITY;
ALTER TABLE weekly_reports ENABLE ROW LEVEL SECURITY;
ALTER TABLE final_reports ENABLE ROW LEVEL SECURITY;


-- Profiles
CREATE POLICY "Public profiles are viewable by everyone" ON profiles FOR SELECT USING (true);
CREATE POLICY "Users can update own profile" ON profiles FOR UPDATE USING (auth.uid() = id);

-- Companies
CREATE POLICY "Companies are viewable by everyone" ON companies FOR SELECT USING (true);
-- Only admin can manage companies (assuming admin role is handled in app logic or custom JWT claims, simplified here)
CREATE POLICY "Admins can insert companies" ON companies FOR INSERT WITH CHECK (EXISTS (SELECT 1 FROM profiles WHERE id = auth.uid() AND role = 'admin'));
CREATE POLICY "Admins can update companies" ON companies FOR UPDATE USING (EXISTS (SELECT 1 FROM profiles WHERE id = auth.uid() AND role = 'admin'));

-- Students
CREATE POLICY "Students are viewable by lecturers and admins" ON students FOR SELECT 
USING (
  EXISTS (SELECT 1 FROM profiles WHERE id = auth.uid() AND role IN ('admin', 'lecturer'))
  OR id = auth.uid()
);
CREATE POLICY "Students can update own info" ON students FOR UPDATE USING (id = auth.uid());
CREATE POLICY "Admins can manage students" ON students FOR ALL USING (EXISTS (SELECT 1 FROM profiles WHERE id = auth.uid() AND role = 'admin'));

-- Weekly Reports
CREATE POLICY "Students can view and insert own reports" ON weekly_reports FOR SELECT USING (student_id = auth.uid());
CREATE POLICY "Students can insert own reports" ON weekly_reports FOR INSERT WITH CHECK (student_id = auth.uid());
CREATE POLICY "Students can update own pending reports" ON weekly_reports FOR UPDATE USING (student_id = auth.uid() AND status = 'pending');

CREATE POLICY "Lecturers can view assigned student reports" ON weekly_reports FOR SELECT 
USING (
  EXISTS (SELECT 1 FROM students WHERE students.id = weekly_reports.student_id AND students.lecturer_id = auth.uid())
);
CREATE POLICY "Lecturers can update assigned student reports" ON weekly_reports FOR UPDATE 
USING (
  EXISTS (SELECT 1 FROM students WHERE students.id = weekly_reports.student_id AND students.lecturer_id = auth.uid())
);

CREATE POLICY "Admins can view all reports" ON weekly_reports FOR SELECT USING (EXISTS (SELECT 1 FROM profiles WHERE id = auth.uid() AND role = 'admin'));

-- Final Reports (Similar to Weekly Reports)
CREATE POLICY "Students can view and insert own final reports" ON final_reports FOR SELECT USING (student_id = auth.uid());
CREATE POLICY "Students can insert own final reports" ON final_reports FOR INSERT WITH CHECK (student_id = auth.uid());
CREATE POLICY "Students can update own pending final reports" ON final_reports FOR UPDATE USING (student_id = auth.uid() AND status = 'pending');

CREATE POLICY "Lecturers can view assigned student final reports" ON final_reports FOR SELECT 
USING (
  EXISTS (SELECT 1 FROM students WHERE students.id = final_reports.student_id AND students.lecturer_id = auth.uid())
);
CREATE POLICY "Lecturers can update assigned student final reports" ON final_reports FOR UPDATE 
USING (
  EXISTS (SELECT 1 FROM students WHERE students.id = final_reports.student_id AND students.lecturer_id = auth.uid())
);

CREATE POLICY "Admins can view all final reports" ON final_reports FOR SELECT USING (EXISTS (SELECT 1 FROM profiles WHERE id = auth.uid() AND role = 'admin'));
