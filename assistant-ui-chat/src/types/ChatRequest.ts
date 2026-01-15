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
}

/**
 * SSE event: data (streamed content)
 */
export interface ChatData {
  threadId: string;
  text: string;
}

/**
 * SSE event: done (sent last)
 */
export interface ChatDone {
  threadId: string;
  title?: string | null;
  isNewConversation: boolean;
}

/**
 * SSE event: error
 */
export interface ChatError {
  error: string;
  code: string;
}