// =============================================================================
// MarkdownText Component - Using @assistant-ui/react-markdown
// Based on official assistant-ui documentation for v0.11.x
// =============================================================================

import {
  type CodeHeaderProps,
  MarkdownTextPrimitive,
  unstable_memoizeMarkdownComponents as memoizeMarkdownComponents,
  useIsMarkdownCodeBlock,
} from "@assistant-ui/react-markdown";
import remarkGfm from "remark-gfm";
import { type FC, memo, useState } from "react";
import { CheckIcon, CopyIcon } from "lucide-react";
import { cn } from "@/lib/utils";

// =============================================================================
// Main MarkdownText Component
// =============================================================================
const MarkdownTextImpl: FC = () => {
  return (
    <MarkdownTextPrimitive
      remarkPlugins={[remarkGfm]}
      className="aui-md"
      components={defaultComponents}
    />
  );
};

export const MarkdownText = memo(MarkdownTextImpl);

// =============================================================================
// Code Header with Copy Button
// =============================================================================
const CodeHeader: FC<CodeHeaderProps> = ({ language, code }) => {
  const { isCopied, copyToClipboard } = useCopyToClipboard();

  const onCopy = () => {
    if (!code || isCopied) return;
    copyToClipboard(code);
  };

  return (
    <div className="aui-code-header-root">
      <span className="aui-code-header-language">{language || "text"}</span>
      <button onClick={onCopy} className="aui-code-copy-btn">
        {isCopied ? (
          <>
            <CheckIcon className="aui-code-copy-icon" /> Copied
          </>
        ) : (
          <>
            <CopyIcon className="aui-code-copy-icon" /> Copy
          </>
        )}
      </button>
    </div>
  );
};

// =============================================================================
// Copy to Clipboard Hook
// =============================================================================
const useCopyToClipboard = ({ copiedDuration = 3000 }: { copiedDuration?: number } = {}) => {
  const [isCopied, setIsCopied] = useState<boolean>(false);

  const copyToClipboard = (value: string) => {
    if (!value) return;

    navigator.clipboard.writeText(value).then(() => {
      setIsCopied(true);
      setTimeout(() => setIsCopied(false), copiedDuration);
    });
  };

  return { isCopied, copyToClipboard };
};

// =============================================================================
// Memoized Markdown Components
// =============================================================================
const defaultComponents = memoizeMarkdownComponents({
  h1: ({ className, ...props }) => (
    <h1
      className={cn("aui-md-h1 mb-6 scroll-m-20 font-bold text-2xl tracking-tight", className)}
      {...props}
    />
  ),
  h2: ({ className, ...props }) => (
    <h2
      className={cn("aui-md-h2 mt-6 mb-4 scroll-m-20 font-semibold text-xl tracking-tight", className)}
      {...props}
    />
  ),
  h3: ({ className, ...props }) => (
    <h3
      className={cn("aui-md-h3 mt-4 mb-3 scroll-m-20 font-semibold text-lg tracking-tight", className)}
      {...props}
    />
  ),
  h4: ({ className, ...props }) => (
    <h4
      className={cn("aui-md-h4 mt-4 mb-2 scroll-m-20 font-semibold text-base", className)}
      {...props}
    />
  ),
  p: ({ className, ...props }) => (
    <p className={cn("aui-md-p mb-4 leading-7 last:mb-0", className)} {...props} />
  ),
  a: ({ className, ...props }) => (
    <a
      className={cn("aui-md-a font-medium text-blue-600 underline underline-offset-4", className)}
      target="_blank"
      rel="noopener noreferrer"
      {...props}
    />
  ),
  blockquote: ({ className, ...props }) => (
    <blockquote
      className={cn("aui-md-blockquote border-l-4 border-blue-500 pl-4 italic my-4", className)}
      {...props}
    />
  ),
  ul: ({ className, ...props }) => (
    <ul className={cn("aui-md-ul my-4 ml-6 list-disc [&>li]:mt-2", className)} {...props} />
  ),
  ol: ({ className, ...props }) => (
    <ol className={cn("aui-md-ol my-4 ml-6 list-decimal [&>li]:mt-2", className)} {...props} />
  ),
  hr: ({ className, ...props }) => (
    <hr className={cn("aui-md-hr my-6 border-t border-gray-200", className)} {...props} />
  ),
  table: ({ className, ...props }) => (
    <div className="aui-md-table-wrapper my-4 w-full overflow-x-auto">
      <table className={cn("aui-md-table w-full border-collapse", className)} {...props} />
    </div>
  ),
  th: ({ className, ...props }) => (
    <th
      className={cn(
        "aui-md-th bg-gray-100 px-4 py-2 text-left font-semibold border border-gray-200",
        className
      )}
      {...props}
    />
  ),
  td: ({ className, ...props }) => (
    <td
      className={cn("aui-md-td px-4 py-2 border border-gray-200", className)}
      {...props}
    />
  ),
  tr: ({ className, ...props }) => (
    <tr className={cn("aui-md-tr even:bg-gray-50", className)} {...props} />
  ),
  pre: ({ className, ...props }) => (
    <pre
      className={cn(
        "aui-md-pre overflow-x-auto rounded-b-lg bg-slate-900 p-4 text-slate-100 text-sm",
        className
      )}
      {...props}
    />
  ),
  code: function Code({ className, ...props }) {
    const isCodeBlock = useIsMarkdownCodeBlock();
    return (
      <code
        className={cn(
          !isCodeBlock && "aui-md-inline-code rounded bg-gray-100 px-1.5 py-0.5 font-mono text-sm",
          className
        )}
        {...props}
      />
    );
  },
  CodeHeader,
});

export default MarkdownText;
