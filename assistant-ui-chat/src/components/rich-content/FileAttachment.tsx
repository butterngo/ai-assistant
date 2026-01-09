// src/components/rich-content/FileAttachment.tsx
import "./FileAttachment.css";

interface FileAttachmentProps {
  file: {
    name: string;
    url?: string;
    size?: number;
    type?: string;
  };
}

export function FileAttachment({ file }: FileAttachmentProps) {
  const { name, url, size, type } = file;

  const getFileIcon = (type?: string): string => {
    if (!type) return "📄";
    if (type.startsWith("image/")) return "🖼️";
    if (type.startsWith("video/")) return "🎬";
    if (type.startsWith("audio/")) return "🎵";
    if (type.includes("pdf")) return "📕";
    if (type.includes("word") || type.includes("document")) return "📘";
    if (type.includes("excel") || type.includes("spreadsheet")) return "📗";
    if (type.includes("zip") || type.includes("archive")) return "📦";
    if (type.includes("json") || type.includes("javascript") || type.includes("typescript")) return "📜";
    return "📄";
  };

  const formatSize = (bytes?: number): string => {
    if (!bytes) return "";
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  };

  const handleDownload = () => {
    if (url) {
      window.open(url, "_blank");
    }
  };

  return (
    <div className="file-attachment" onClick={handleDownload}>
      <div className="file-icon">{getFileIcon(type)}</div>
      <div className="file-info">
        <div className="file-name">{name}</div>
        {size && <div className="file-size">{formatSize(size)}</div>}
      </div>
      {url && (
        <button className="download-btn" title="Download">
          <DownloadIcon />
        </button>
      )}
    </div>
  );
}

function DownloadIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4" />
      <polyline points="7 10 12 15 17 10" />
      <line x1="12" y1="15" x2="12" y2="3" />
    </svg>
  );
}

export default FileAttachment;
