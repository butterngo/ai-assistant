import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "./layout/Applayout";
import { SettingsLayout } from "./layout/SettingsLayout";
import {
  ChatPage,
  TestChatPage,
  AgentsPage,
  AgentSkillsPage,
  ToolsPage,
  KnowledgeBasePage,
  ProfilePage,
} from "./pages";
import { conversationLoader } from "./loaders/Conversationloader";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      {
        index: true,
        element: <ChatPage />,
      },
      {
        path: "conversation/:threadId",
        element: <ChatPage />,
        loader: conversationLoader,
      },
      {
        path: "conversations",
        element: <Navigate to="/" replace />,
      }
    ],
  },
  {
    // Test Chat - Standalone route (outside AppLayout and SettingsLayout)
    path: "/test-chat",
    element: <TestChatPage />,
  },
  {
    path: "/settings",
    element: <SettingsLayout />,
    children: [
      {
        index: true,
        element: <Navigate to="agents" replace />,
      },
      {
        path: "agents",
        element: <AgentsPage />,
      },
      {
        // Skills inside an agent
        path: "agents/:agentId/skills",
        element: <AgentSkillsPage />,
      },
      {
        path: "tools",
        element: <ToolsPage />,
      },
      {
        path: "knowledge-base",
        element: <KnowledgeBasePage />,
      },
      {
        path: "profile",
        element: <ProfilePage />,
      }
    ],
  },
]);