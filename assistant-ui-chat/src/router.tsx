import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "./layout/Applayout";
import { SettingsLayout } from "./layout/SettingsLayout";
import {
  ChatPage,
  CategoriesPage,
  CategorySkillsPage,
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
        path: "conversation/:conversationId",
        element: <ChatPage />,
        loader: conversationLoader,
      },
      {
        path: "conversations",
        element: <Navigate to="/" replace />,
      },
    ],
  },
  {
    path: "/settings",
    element: <SettingsLayout />,
    children: [
      {
        index: true,
        element: <Navigate to="categories" replace />,
      },
      {
        path: "categories",
        element: <CategoriesPage />,
      },
      {
        // Skills inside a category
        path: "categories/:categoryId/skills",
        element: <CategorySkillsPage />,
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
      },
    ],
  },
]);