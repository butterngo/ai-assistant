-- Create extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create table
CREATE TABLE intent_classifications (
    id BIGSERIAL PRIMARY KEY,
    user_message TEXT NOT NULL,
    embedding VECTOR(1536), -- Adjust dimension based on your embedding model
    specialist VARCHAR(50) NOT NULL,
    reason TEXT NOT NULL,
    confidence DOUBLE PRECISION NOT NULL,
    session_id VARCHAR(100),
    user_id VARCHAR(100),
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Indexes
CREATE INDEX idx_intent_embedding ON intent_classifications 
USING hnsw (embedding vector_cosine_ops);

CREATE INDEX idx_intent_specialist ON intent_classifications (specialist);
CREATE INDEX idx_intent_created_at ON intent_classifications (created_at DESC);
CREATE INDEX idx_intent_session ON intent_classifications (session_id);