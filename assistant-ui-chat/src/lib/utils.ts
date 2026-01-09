import { type ClassValue, clsx } from "clsx";

// Simple cn function without clsx dependency
export function cn(...inputs: (string | undefined | null | false)[]): string {
  return inputs.filter(Boolean).join(" ");
}
