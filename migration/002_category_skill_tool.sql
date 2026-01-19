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
    updated_at TIMESTAMP DEFAULT NOW()
);

INSERT INTO categories (id, code, name, description, created_at, updated_at)
VALUES
    ('00000000-0000-0000-0000-000000000001', 'agent-general', 'General', 'General purpose, no specific specialization', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000002', 'agent-po', 'Product Owner', 'Product ownership, backlog management, user stories, requirements', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000003', 'agent-pm', 'Project Manager', 'Project planning, scheduling, risk management, stakeholder communication', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000004', 'agent-sa', 'Software Architect', 'System design, architecture patterns, C4 diagrams, technical decisions', NOW(), NOW()),
    ('00000000-0000-0000-0000-000000000005', 'agent-sbd', 'Backend Developer', 'Coding, debugging, testing, code review, CI/CD', NOW(), NOW()),
	('00000000-0000-0000-0000-000000000006', 'agent-sfd', 'Frontend Developer', 'Coding, debugging, testing, code review, CI/CD', NOW(), NOW()),
	('00000000-0000-0000-0000-000000000007', 'agent-devops', 'Devops Engineer', 'Coding, debugging, testing, code review, CI/CD', NOW(), NOW())