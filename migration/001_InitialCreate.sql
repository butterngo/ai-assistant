-- PostgreSQL Migration Script for ChatMessageStore
-- Run this to create the required tables

-- Create chat_messages table
CREATE TABLE IF NOT EXISTS chat_messages (
    id UUID PRIMARY KEY,
    thread_id VARCHAR(64) NOT NULL,
    message_id VARCHAR(64) NOT NULL,
    role VARCHAR(32) NOT NULL,
    content TEXT NOT NULL DEFAULT '',
    serialized_message JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    sequence_number BIGINT NOT NULL
);

-- Create chat_threads table (optional, for thread metadata)
CREATE TABLE IF NOT EXISTS chat_threads (
    id UUID PRIMARY KEY,
    thread_id VARCHAR(64) NOT NULL,
    title VARCHAR(256),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    serialized_thread_state JSONB,
    user_id VARCHAR(128)
);

-- Indexes for chat_messages
CREATE INDEX IF NOT EXISTS ix_chat_messages_thread_sequence 
    ON chat_messages (thread_id, sequence_number);

CREATE INDEX IF NOT EXISTS ix_chat_messages_thread_id 
    ON chat_messages (thread_id);

CREATE INDEX IF NOT EXISTS ix_chat_messages_created_at 
    ON chat_messages (created_at);

-- Indexes for chat_threads
CREATE UNIQUE INDEX IF NOT EXISTS ix_chat_threads_thread_id 
    ON chat_threads (thread_id);

CREATE INDEX IF NOT EXISTS ix_chat_threads_user_id 
    ON chat_threads (user_id);

CREATE INDEX IF NOT EXISTS ix_chat_threads_updated_at 
    ON chat_threads (updated_at);

-- Optional: Create a GIN index on serialized_message for JSONB queries
CREATE INDEX IF NOT EXISTS ix_chat_messages_serialized_gin 
    ON chat_messages USING GIN (serialized_message);

-- Comment on tables
COMMENT ON TABLE chat_messages IS 'Stores individual chat messages for Microsoft Agent Framework';
COMMENT ON TABLE chat_threads IS 'Stores conversation thread metadata and state';

-- Comment on columns
COMMENT ON COLUMN chat_messages.thread_id IS 'Groups messages belonging to the same conversation';
COMMENT ON COLUMN chat_messages.sequence_number IS 'Ordering sequence within the thread';
COMMENT ON COLUMN chat_messages.serialized_message IS 'Full ChatMessage object serialized as JSON';
COMMENT ON COLUMN chat_threads.serialized_thread_state IS 'AgentThread state for resumption';
