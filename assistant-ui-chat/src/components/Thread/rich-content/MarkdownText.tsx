// src/components/rich-content/MarkdownText.tsx
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { CodeBlock } from "./CodeBlock";
import "./MarkdownText.css";

interface MarkdownTextProps {
  text: string;
}

export function MarkdownText({ text }: MarkdownTextProps) {
  return (
    <div className="markdown-content">
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          // Code blocks with syntax highlighting
          code({ node, inline, className, children, ...props }) {
            const match = /language-(\w+)/.exec(className || "");
            const language = match ? match[1] : "";
            const codeString = String(children).replace(/\n$/, "");

            if (!inline && (language || codeString.includes("\n"))) {
              return <CodeBlock language={language} code={codeString} />;
            }

            return (
              <code className="inline-code" {...props}>
                {children}
              </code>
            );
          },

          // Links open in new tab
          a({ href, children }) {
            return (
              <a href={href} target="_blank" rel="noopener noreferrer">
                {children}
              </a>
            );
          },

          // Tables
          table({ children }) {
            return (
              <div className="table-wrapper">
                <table className="markdown-table">{children}</table>
              </div>
            );
          },

          // Images
          img({ src, alt }) {
            return (
              <figure className="markdown-figure">
                <img src={src} alt={alt} className="markdown-image" />
                {alt && <figcaption>{alt}</figcaption>}
              </figure>
            );
          },

          // Blockquotes
          blockquote({ children }) {
            return <blockquote className="markdown-blockquote">{children}</blockquote>;
          },

          // Lists
          ul({ children }) {
            return <ul className="markdown-list">{children}</ul>;
          },

          ol({ children }) {
            return <ol className="markdown-list ordered">{children}</ol>;
          },

          // Horizontal rule
          hr() {
            return <hr className="markdown-hr" />;
          },
        }}
      >
        {text}
      </ReactMarkdown>
    </div>
  );
}

export default MarkdownText;
