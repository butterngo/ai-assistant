-- PostgreSQL Migration Script for ChatMessageStore
-- Run this to create the required tables

-- Create chat_messages table
CREATE TABLE IF NOT EXISTS chat_messages (
    id UUID PRIMARY KEY,
    thread_id UUID NOT NULL,
    role VARCHAR(32) NOT NULL,
    content TEXT NOT NULL DEFAULT '',
    serialized_message JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    sequence_number BIGINT NOT NULL
);

-- Create chat_threads table (optional, for thread metadata)
CREATE TABLE IF NOT EXISTS chat_threads (
    id UUID PRIMARY KEY,
    title VARCHAR(256),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    serialized_thread_state JSONB,
    user_id VARCHAR(128)
);
