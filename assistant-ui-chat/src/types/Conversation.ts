
export interface Conversation {
  id: string;
  threadId: string;
  title: string | null;
  createdAt: string;
  updatedAt: string;
  userId?: string | null;
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
  messageCount: number;
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