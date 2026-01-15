import { axiosClient } from "./axiosClient";
import type {
  Conversation,
  ConversationDetail,
  GetConversationsParams,
  PagedResult,
} from "../types";

export const conversationsClient = {
  /**
   * Get all conversations (paginated)
   */
  async getAll(params?: GetConversationsParams): Promise<Conversation[]> {
    const { data } = await axiosClient.get<PagedResult<Conversation>>(
      "/api/conversations",
      { params }
    );
    return data.items;
  },

  /**
   * Get conversation by ID with messages
   */
  async getById(id: string): Promise<ConversationDetail> {
    const { data } = await axiosClient.get<ConversationDetail>(
      `/api/conversations/${id}`
    );
    return data;
  },

  /**
   * Delete a conversation and all its messages
   */
  async delete(id: string): Promise<void> {
    await axiosClient.delete(`/api/conversations/${id}`);
  },
};