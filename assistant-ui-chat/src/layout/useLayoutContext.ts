import { useOutletContext } from "react-router-dom";
import type { Conversation } from "../types";

export interface LayoutContextType {
  sidebarOpen: boolean;
  onToggleSidebar: () => void;
  addConversation: (conversation: Conversation) => void;
  navigateToConversation: (id: string) => void;
}

export function useLayoutContext() {
  return useOutletContext<LayoutContextType>();
}