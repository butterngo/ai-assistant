import { DebugContext } from './Debugcontext';

/**
 * Request body for chat endpoint
 */
export interface ChatRequest {
  message: string;
  threadId?: string;
}

/**
 * SSE event: metadata (sent first)
 */
export interface ChatMetadata {
  threadId: string;
  title?: string | null;
  isNewConversation: boolean;
  isTestMode: boolean;
  isStreaming: boolean;
}

/**
 * SSE event: data (streamed content)
 */
export interface ChatData {
  threadId: string;
  text: string;
  isStreaming: boolean;
}

/**
 * SSE event: done (sent last)
 */
export interface ChatDone {
  threadId: string;
  title?: string | null;
  isNewConversation: boolean;
  isStreaming: boolean;
  isTestMode: boolean;
  debugContext?: DebugContext;
}

/**
 * SSE event: error
 */
export interface ChatError {
  threadId: string;
  error: string;
  code: string;
  isStreaming: boolean;
}