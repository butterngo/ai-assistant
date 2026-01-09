// src/components/rich-content/ImageContent.tsx
import { useState } from "react";
import "./ImageContent.css";

interface ImageContentProps {
  image: {
    url: string;
    alt?: string;
    width?: number;
    height?: number;
  };
}

export function ImageContent({ image }: ImageContentProps) {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);
  const [isExpanded, setIsExpanded] = useState(false);

  const handleLoad = () => setIsLoading(false);
  const handleError = () => {
    setIsLoading(false);
    setError(true);
  };

  if (error) {
    return (
      <div className="image-error">
        <span>🖼️</span>
        <p>Failed to load image</p>
      </div>
    );
  }

  return (
    <figure className="image-content">
      {isLoading && (
        <div className="image-loading">
          <div className="spinner" />
        </div>
      )}
      <img
        src={image.url}
        alt={image.alt || "Image"}
        width={image.width}
        height={image.height}
        onLoad={handleLoad}
        onError={handleError}
        onClick={() => setIsExpanded(true)}
        className={isLoading ? "hidden" : ""}
      />
      {image.alt && <figcaption>{image.alt}</figcaption>}

      {/* Lightbox */}
      {isExpanded && (
        <div className="lightbox" onClick={() => setIsExpanded(false)}>
          <button className="lightbox-close">✕</button>
          <img src={image.url} alt={image.alt || "Image"} />
        </div>
      )}
    </figure>
  );
}

export default ImageContent;
