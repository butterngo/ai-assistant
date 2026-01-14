-- Categories
CREATE TABLE categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Skills
CREATE TABLE skills (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id UUID NOT NULL REFERENCES categories(id),
    name VARCHAR(100) NOT NULL,
    system_prompt TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    UNIQUE(category_id, name)
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
