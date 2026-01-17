-- Categories
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Skills
CREATE TABLE skills (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	code VARCHAR(100) NOT NULL UNIQUE,
    category_id UUID NOT NULL REFERENCES categories(id),
    name VARCHAR(100) NOT NULL,
	description TEXT NOT NULL,
    system_prompt TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
	
    UNIQUE(category_id, code)
);

-- Tools
CREATE TABLE tools (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    skill_id UUID NOT NULL REFERENCES skills(id),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,  -- 'mcp', 'openapi'
    endpoint VARCHAR(500) NOT NULL,
    description TEXT,
    config JSONB DEFAULT '{}',
    is_prefetch BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(skill_id, name)
);

-- Indexes
CREATE INDEX idx_skills_category_id ON skills(category_id);
CREATE INDEX idx_tools_skill_id ON tools(skill_id);
CREATE INDEX idx_tools_type ON tools(type);

-- Enable pgcrypto if you are on an older Postgres version and gen_random_uuid() fails
-- CREATE EXTENSION IF NOT EXISTS "pgcrypto";

INSERT INTO categories (id, code, name, description, created_at, updated_at)
VALUES
    ('00000000-0000-0000-0000-000000000001', 'anget-general', 'General', 'General purpose, no specific specialization', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000002', 'anget-po', 'Product Owner', 'Product ownership, backlog management, user stories, requirements', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000003', 'anget-pm', 'Project Manager', 'Project planning, scheduling, risk management, stakeholder communication', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000004', 'anget-sa', 'Software Architect', 'System design, architecture patterns, C4 diagrams, technical decisions', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000005', 'anget-sbd', 'Backend Developer', 'Coding, debugging, testing, code review, CI/CD', NOW(), NOW()),
	('00000000-0000-0000-0000-000000000006', 'anget-sfd', 'Frontend Developer', 'Coding, debugging, testing, code review, CI/CD', NOW(), NOW()),
	('00000000-0000-0000-0000-000000000007', 'anget-devops', 'Devops Engineer', 'Coding, debugging, testing, code review, CI/CD', NOW(), NOW()),
ON CONFLICT (code) DO NOTHING;