-- Initial Schema for Internship Management System

-- 1. Enum types
CREATE TYPE user_role AS ENUM ('admin', 'lecturer', 'student', 'company');
CREATE TYPE student_status AS ENUM ('unregistered', 'pending_approval', 'outline_review', 'in_progress', 'warning_delayed', 'completed');
CREATE TYPE report_status AS ENUM ('pending', 'approved', 'revision_requested', 'graded');

-- 2. Profiles (extends auth.users)
CREATE TABLE profiles (
  id UUID REFERENCES auth.users(id) ON DELETE CASCADE PRIMARY KEY,
  email TEXT NOT NULL UNIQUE,
  full_name TEXT NOT NULL,
  role user_role NOT NULL DEFAULT 'student',
  avatar_url TEXT,
  phone TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 3. Companies
CREATE TABLE companies (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT NOT NULL,
  short_name TEXT,
  tax_code TEXT,
  address TEXT,
  website TEXT,
  contact_person TEXT,
  contact_email TEXT,
  contact_phone TEXT,
  is_partner BOOLEAN DEFAULT false,
  logo_url TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 4. Lecturers
CREATE TABLE lecturers (
  id UUID REFERENCES profiles(id) ON DELETE CASCADE PRIMARY KEY,
  code TEXT UNIQUE NOT NULL,
  title TEXT,
  department TEXT,
  max_students INTEGER DEFAULT 15,
  research_interests TEXT[]
);

-- 5. Internship Periods
CREATE TABLE internship_periods (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT NOT NULL,
  academic_year TEXT NOT NULL,
  start_date DATE NOT NULL,
  end_date DATE NOT NULL,
  total_weeks INTEGER DEFAULT 12,
  registration_deadline DATE,
  outline_deadline DATE,
  midterm_deadline DATE,
  final_deadline DATE,
  is_active BOOLEAN DEFAULT false,
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 6. Students
CREATE TABLE students (
  id UUID REFERENCES profiles(id) ON DELETE CASCADE PRIMARY KEY,
  code TEXT UNIQUE NOT NULL,
  class_name TEXT,
  major TEXT,
  gpa DECIMAL(3, 2),
  status student_status DEFAULT 'unregistered',
  
  -- Placement Info
  company_id UUID REFERENCES companies(id),
  company_name_temp TEXT, -- For self-found companies not yet in DB
  position TEXT,
  mentor_name TEXT,
  mentor_phone TEXT,
  mentor_email TEXT,
  is_self_found BOOLEAN DEFAULT false,
  
  -- Supervisor Info
  lecturer_id UUID REFERENCES lecturers(id),
  
  -- Current Period
  period_id UUID REFERENCES internship_periods(id)
);

-- 7. Weekly Reports
CREATE TABLE weekly_reports (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  student_id UUID REFERENCES students(id) ON DELETE CASCADE,
  week_number INTEGER NOT NULL,
  start_date DATE,
  end_date DATE,
  tasks_completed TEXT NOT NULL,
  skills_learned TEXT,
  difficulties TEXT,
  attendance_days INTEGER,
  
  status report_status DEFAULT 'pending',
  lecturer_score DECIMAL(4, 2),
  lecturer_feedback TEXT,
  company_feedback TEXT,
  
  submitted_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 8. Final Reports
CREATE TABLE final_reports (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  student_id UUID REFERENCES students(id) ON DELETE CASCADE,
  type TEXT NOT NULL CHECK (type IN ('midterm', 'final')),
  title TEXT NOT NULL,
  summary TEXT,
  file_url TEXT NOT NULL,
  github_url TEXT,
  
  score DECIMAL(4, 2),
  comment TEXT,
  status report_status DEFAULT 'pending',
  
  submitted_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Triggers for updated_at
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
   NEW.updated_at = NOW();
   RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_profiles_modtime BEFORE UPDATE ON profiles FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();
CREATE TRIGGER update_companies_modtime BEFORE UPDATE ON companies FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();
CREATE TRIGGER update_weekly_reports_modtime BEFORE UPDATE ON weekly_reports FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();
CREATE TRIGGER update_final_reports_modtime BEFORE UPDATE ON final_reports FOR EACH ROW EXECUTE PROCEDURE update_updated_at_column();
