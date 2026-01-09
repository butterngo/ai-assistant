import { useMemo } from "react";
import type { ChatModelAdapter, ChatModelRunOptions } from "@assistant-ui/react";

// =============================================================================
// Types
// =============================================================================
export interface ChatStreamAdapterOptions {
  /** API endpoint URL */
  api: string;
  /** Current conversation ID */
  conversationId: string;
  /** Additional headers for the request */
  headers?: Record<string, string>;
  /** Callback when streaming starts */
  onStart?: () => void;
  /** Callback for each chunk received */
  onChunk?: (chunk: string) => void;
  /** Callback when streaming completes */
  onFinish?: (fullText: string) => void;
  /** Callback when an error occurs */
  onError?: (error: Error) => void;
}

// =============================================================================
// SSE Adapter Factory
// =============================================================================
function createChatStreamAdapter(options: ChatStreamAdapterOptions): ChatModelAdapter {
  const { api, conversationId, headers = {}, onStart, onChunk, onFinish, onError } = options;

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
          headers: { "Content-Type": "application/json", ...headers },
          body: JSON.stringify({
            message: textContent.text,
            conversationId,
          }),
          signal: abortSignal,
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        if (!response.body) {
          throw new Error("Response body is null");
        }

        const reader = response.body.getReader();
        const decoder = new TextDecoder();
        let buffer = "";
        let fullText = "";

        try {
          while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            buffer += decoder.decode(value, { stream: true });
            const lines = buffer.split("\n\n");
            buffer = lines.pop() || "";

            for (const line of lines) {
              if (line.startsWith("data: ")) {
                const data = line.slice(6);
                if (data === "[DONE]") break;

                try {
                  const json = JSON.parse(data);
                  if (json.text) {
                    fullText += json.text;
                    onChunk?.(json.text);
                    yield { content: [{ type: "text" as const, text: fullText }] };
                  }
                  if (json.error) {
                    throw new Error(json.error);
                  }
                } catch (e) {
                  // Only warn for actual parse errors, not [DONE]
                  if (e instanceof SyntaxError) {
                    console.warn("SSE parse error:", data);
                  } else {
                    throw e;
                  }
                }
              }
            }
          }
        } finally {
          reader.releaseLock();
        }

        onFinish?.(fullText);
        return { content: [{ type: "text" as const, text: fullText }] };

      } catch (error) {
        const err = error instanceof Error ? error : new Error("Unknown error");
        onError?.(err);
        throw err;
      }
    },
  };
}

export function useChatStreamAdapter(options: ChatStreamAdapterOptions): ChatModelAdapter {
  const { api, conversationId, headers, onStart, onChunk, onFinish, onError } = options;

  // Memoize adapter to prevent unnecessary re-creation
  // Only recreate when api or conversationId changes
  const adapter = useMemo(
    () => createChatStreamAdapter({
      api,
      conversationId,
      headers,
      onStart,
      onChunk,
      onFinish,
      onError,
    }),
    [api, conversationId] // Callbacks are intentionally excluded to avoid churn
  );

  return adapter;
}

// Export factory for advanced use cases
export { createChatStreamAdapter };