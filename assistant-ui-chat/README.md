# Assistant UI Chat - v0.11.x

A React chat interface for Azure AI Foundry agents using `@assistant-ui/react` v0.11.x with best practices.

## Features

- ✅ SSE streaming support for Azure AI Foundry
- ✅ Markdown rendering with `@assistant-ui/react-markdown`
- ✅ Code blocks with syntax highlighting and copy button
- ✅ ViewportFooter with ScrollToBottom (v0.11.x feature)
- ✅ Message branching and editing
- ✅ Responsive design with Tailwind CSS

## Quick Start

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

## Dependencies

```json
{
  "@assistant-ui/react": "^0.11.53",
  "@assistant-ui/react-markdown": "^0.11.9",
  "lucide-react": "^0.460.0",
  "remark-gfm": "^4.0.0"
}
```

## Project Structure

```
src/
├── App.tsx                    # Main app with SSE adapter
├── index.css                  # Global styles + Tailwind
├── main.tsx                   # Entry point
├── lib/
│   └── utils.ts               # Helper functions
└── components/
    ├── Thread.tsx             # Main chat thread component
    ├── Thread.css             # Thread styles
    └── MarkdownText.tsx       # Markdown rendering component
```

## API Configuration

The app expects your backend at `http://localhost:5050/chat/stream` with this format:

**Request:**
```json
POST /chat/stream
Content-Type: application/json

{
  "message": "Hello!",
  "threadId": "uuid-here"
}
```

**Response (SSE):**
```
data: {"text":"Hello"}

data: {"text":"! How"}

data: {"text":" can I help?"}

data: [DONE]
```

## Key v0.11.x Features Used

### ViewportFooter
The scroll-to-bottom button and composer are now inside `ViewportFooter`, which is a sticky element at the bottom of the viewport. This is the official pattern for v0.11.x.

```tsx
<ThreadPrimitive.Viewport>
  <ThreadPrimitive.Messages ... />
  
  <ThreadPrimitive.ViewportFooter>
    <ThreadScrollToBottom />
    <Composer />
  </ThreadPrimitive.ViewportFooter>
</ThreadPrimitive.Viewport>
```

### @assistant-ui/react-markdown
Uses the official markdown package instead of raw `react-markdown`:

```tsx
import { MarkdownTextPrimitive } from "@assistant-ui/react-markdown";

<MarkdownTextPrimitive
  remarkPlugins={[remarkGfm]}
  components={customComponents}
/>
```

## Customization

### Change API endpoint
Edit `src/App.tsx`:
```tsx
const adapter = createSSEAdapter("YOUR_API_URL");
```

### Customize styles
Edit `src/components/Thread.css` for component styles or `src/index.css` for global styles.

### Add syntax highlighting
Install `@assistant-ui/react-syntax-highlighter` for code syntax highlighting:

```bash
npm install @assistant-ui/react-syntax-highlighter shiki
```

## License

MIT
