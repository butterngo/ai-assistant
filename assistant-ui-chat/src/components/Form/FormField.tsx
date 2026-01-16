import { type FC, type ReactNode } from "react";
import "./FormField.css";

// =============================================================================
// Types
// =============================================================================

export interface FormFieldProps {
  label: string;
  htmlFor?: string;
  required?: boolean;
  error?: string;
  hint?: string;
  children: ReactNode;
}

// =============================================================================
// Component
// =============================================================================

export const FormField: FC<FormFieldProps> = ({
  label,
  htmlFor,
  required = false,
  error,
  hint,
  children,
}) => {
  return (
    <div className={`form-field ${error ? "has-error" : ""}`}>
      <label htmlFor={htmlFor}>
        {label}
        {required && <span className="required-mark">*</span>}
      </label>
      {children}
      {error && <span className="field-error">{error}</span>}
      {hint && !error && <span className="field-hint">{hint}</span>}
    </div>
  );
};

export default FormField;