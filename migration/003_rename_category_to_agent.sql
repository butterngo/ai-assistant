-- Migration: Rename Category to Agent
-- Description: Renames the categories table to agents and updates all references

-- Rename the main table
ALTER TABLE categories RENAME TO agents;

-- Rename the foreign key column in skills table
ALTER TABLE skills RENAME COLUMN category_id TO agent_id;

-- Note: PostgreSQL automatically updates index and constraint names when renaming tables/columns
-- The foreign key constraint and indexes will be automatically updated
