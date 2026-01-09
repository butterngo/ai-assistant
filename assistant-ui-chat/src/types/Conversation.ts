
export interface Conversation {
  /** Unique identifier (UUID) */
  id: string;

  /** Thread ID used for chat persistence */
  threadId: string;

  /** Conversation title (generated from first message) */
  title: string | null;

  /** ISO date string when conversation was created */
  createdAt: string;

  /** ISO date string when conversation was last updated */
  updatedAt: string;

  /** Optional user ID for multi-tenant scenarios */
  userId?: string;

  /** Optional preview text (last message snippet) */
  preview?: string;

  /** Number of messages in conversation */
  messageCount?: number;
}


export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ConversationDetail extends Conversation {
  messages: Message[];
}

export interface Message {
  id: string;
  messageId: string;
  role: "user" | "assistant" | "system";
  content: string;
  createdAt: string;
  sequenceNumber: number;
}

export interface ChatMetadata {
  conversationId: string;
  title: string | null;
  isNewConversation: boolean;
}


export interface ChatData {
  conversationId: string;
  text: string;
}

export interface ChatDone {
  conversationId: string;
  title: string | null;
}

export interface ChatError {
  error: string;
  code: string;
}


export interface ChatRequest {
  conversationId: string | null;
  message: string;
}