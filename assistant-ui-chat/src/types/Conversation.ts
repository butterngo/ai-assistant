
export interface Conversation {
  id: string;
  threadId: string;
  title: string | null;
  createdAt: string;
  updatedAt: string;
  userId?: string | null;
}

export interface Message {
  id: string;
  messageId: string;
  role: "user" | "assistant" | "system";
  content: string;
  createdAt: string;
  sequenceNumber: number;
}

export interface ConversationDetail extends Conversation {
  messages: Message[];
  messageCount: number;
}

export interface GetConversationsParams {
  page?: number;
  pageSize?: number;
}