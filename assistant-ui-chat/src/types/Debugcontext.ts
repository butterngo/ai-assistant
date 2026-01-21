// Updated DebugContext interface based on actual backend response

export interface DebugContext {
  agentId: string;
  agentName: string;
  threadId: string;
  userMessage: string;
  similarityThreshold: number | null;
  
  // Skill routing records
  skillRoutingRecords: SkillRoutingRecord[];
  
  // Instructions from the matched skill
  instructions: string;
  
  // Request and response messages
  requestMessages: RequestMessage[];
  responseMessages: ResponseMessage[];
}

export interface SkillRoutingRecord {
  id: string;
  skillCode: string;
  skillName: string;
  userQueries: string;
  score: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface RequestMessage {
  authorName: string | null;
  createdAt: string | null;
  role: string;
  contents: MessageContent[];
  messageId: string | null;
}

export interface ResponseMessage {
  authorName: string;
  createdAt: string;
  role: string;
  contents: MessageContent[];
  messageId: string;
  additionalProperties: any | null;
}

export interface MessageContent {
  $type: string;
  text: string;
  annotations: any | null;
  additionalProperties: any | null;
}