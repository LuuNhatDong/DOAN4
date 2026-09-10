-- Add JSONB columns to students table for flexible data storage
ALTER TABLE students ADD COLUMN outline JSONB;
ALTER TABLE students ADD COLUMN academic_evaluation JSONB;
ALTER TABLE students ADD COLUMN company_evaluation JSONB;
