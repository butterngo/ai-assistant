import { useMemo } from "react";
import type { ChatModelAdapter, ChatModelRunOptions } from "@assistant-ui/react";
import type {
  ChatMetadata,
  ChatData,
  ChatDone,
  ChatError,
} from "../types";

// =============================================================================
// Types
// =============================================================================

export interface ChatStreamAdapterOptions {
  /** API endpoint URL */
  api: string;
  /** Current threadId ID (null for new thread) */
  threadId: string | null;
  /** Additional headers for the request */
  headers?: Record<string, string>;
  /** Callback when metadata is received (first event) */
  onMetadata?: (metadata: ChatMetadata) => void;
  /** Callback when streaming starts */
  onStart?: () => void;
  /** Callback for each text chunk received */
  onChunk?: (chunk: string) => void;
  /** Callback when streaming completes */
  onDone?: (done: ChatDone) => void;
  /** Callback when an error occurs */
  onError?: (error: ChatError) => void;
}

// =============================================================================
// SSE Event Parser
// =============================================================================

interface ParsedSSEEvent {
  event: string;
  data: string;
}

function parseSSEEvents(buffer: string): { events: ParsedSSEEvent[]; remaining: string } {
  const events: ParsedSSEEvent[] = [];
  const lines = buffer.split("\n");
  
  let currentEvent = "message";
  let currentData = "";
  let i = 0;

  while (i < lines.length) {
    const line = lines[i];

    // Empty line = end of event
    if (line === "") {
      if (currentData) {
        events.push({ event: currentEvent, data: currentData });
        currentEvent = "message";
        currentData = "";
      }
      i++;
      continue;
    }

    // Check if this might be an incomplete event at the end
    if (i === lines.length - 1 && !buffer.endsWith("\n\n")) {
      // Return remaining buffer for next iteration
      const remaining = lines.slice(i).join("\n");
      return { events, remaining };
    }

    if (line.startsWith("event: ")) {
      currentEvent = line.slice(7).trim();
    } else if (line.startsWith("data: ")) {
      currentData = line.slice(6);
    }

    i++;
  }

  return { events, remaining: "" };
}

// =============================================================================
// SSE Adapter Factory
// =============================================================================

function createChatStreamAdapter(options: ChatStreamAdapterOptions): ChatModelAdapter {
  const {
    api,
    threadId,
    headers = {},
    onMetadata,
    onStart,
    onChunk,
    onDone,
    onError,
  } = options;

  return {
    async *run({ messages, abortSignal }: ChatModelRunOptions) {
      // Extract last user message
      const lastUserMessage = messages.filter((m) => m.role === "user").pop();
      if (!lastUserMessage) {
        throw new Error("No user message found");
      }

      const textContent = lastUserMessage.content.find((c) => c.type === "text");
      if (!textContent || textContent.type !== "text") {
        throw new Error("No text content found");
      }

      onStart?.();

      try {
        const response = await fetch(api, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Accept: "text/event-stream",
            ...headers,
          },
          body: JSON.stringify({
            message: textContent.text,
            threadId,
          }),
          signal: abortSignal,
        });

        if (!response.ok) {
          const errorText = await response.text();
          throw new Error(`HTTP ${response.status}: ${errorText}`);
        }

        if (!response.body) {
          throw new Error("Response body is null");
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = "";
        let fullText = "";
        let receivedthreadId = threadId;

        try {
          while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            buffer += decoder.decode(value, { stream: true });
            const { events, remaining } = parseSSEEvents(buffer);
            buffer = remaining;

            for (const { event, data } of events) {
              try {
                const json = JSON.parse(data);

                switch (event) {
                  case "metadata": {
                    const metadata = json as ChatMetadata;
                    receivedthreadId = metadata.threadId;
                    onMetadata?.(metadata);
                    break;
                  }

                  case "data": {
                    const chatData = json as ChatData;
                    fullText += chatData.text;
                    onChunk?.(chatData.text);
                    yield {
                      content: [{ type: "text" as const, text: fullText }],
                    };
                    break;
                  }

                  case "done": {
                    const doneData = json as ChatDone;
                    onDone?.(doneData);
                    break;
                  }

                  case "error": {
                    const errorData = json as ChatError;
                    onError?.(errorData);
                    throw new Error(errorData.error);
                  }

                  default:
                    // Handle legacy format (no event type, just data)
                    if (json.text) {
                      fullText += json.text;
                      onChunk?.(json.text);
                      yield {
                        content: [{ type: "text" as const, text: fullText }],
                      };
                    }
                    if (json.error) {
                      onError?.({ error: json.error, code: "UNKNOWN" });
                      throw new Error(json.error);
                    }
                    break;
                }
              } catch (e) {
                if (e instanceof SyntaxError) {
                  console.warn("SSE parse error:", data);
                } else {
                  throw e;
                }
              }
            }
          }
        } finally {
          reader.releaseLock();
        }

        return { content: [{ type: "text" as const, text: fullText }] };
      } catch (error) {
        const err = error instanceof Error ? error : new Error("Unknown error");
        onError?.({ error: err.message, code: "CLIENT_ERROR" });
        throw err;
      }
    },
  };
}

// =============================================================================
// React Hook
// =============================================================================

export function useChatStreamAdapter(options: ChatStreamAdapterOptions): ChatModelAdapter {
  const { api, threadId, headers, onMetadata, onStart, onChunk, onDone, onError } = options;

  // Memoize adapter - recreate when api or threadId changes
  const adapter = useMemo(
    () =>
      createChatStreamAdapter({
        api,
        threadId,
        headers,
        onMetadata,
        onStart,
        onChunk,
        onDone,
        onError,
      }),
    [api, threadId] // Callbacks intentionally excluded to avoid churn
  );

  return adapter;
}

// =============================================================================
// Exports
// =============================================================================

export { createChatStreamAdapter };
export type { ChatModelAdapter, ChatModelRunOptions };

// Re-export types from Conversation for convenience
export type { ChatMetadata, ChatData, ChatDone, ChatError } from "../types/Conversation";