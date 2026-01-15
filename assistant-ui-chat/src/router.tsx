import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "./layout/Applayout";
import { ChatPage } from "./pages/Chatpage";
import { conversationLoader } from "./loaders/Conversationloader";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      {
        // New conversation (no ID)
        index: true,
        element: <ChatPage />,
      },
      {
        // Existing conversation
        path: "conversation/:threadId",
        element: <ChatPage />,
        loader: conversationLoader,
      },
      {
        // Redirect /conversations to /
        path: "conversations",
        element: <Navigate to="/" replace />,
      },
    ],
  },
]);