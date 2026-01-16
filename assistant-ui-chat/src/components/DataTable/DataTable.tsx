import { type FC, type ReactNode } from "react";
import { SearchIcon, ChevronLeftIcon, ChevronRightIcon } from "lucide-react";
import "./DataTable.css";

// =============================================================================
// Types
// =============================================================================

export interface Column<T> {
  key: string;
  header: string;
  width?: string;
  render?: (item: T) => ReactNode;
}

export interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyField: keyof T;
  loading?: boolean;
  error?: string | null;
  emptyIcon?: ReactNode;
  emptyTitle?: string;
  emptyDescription?: string;
  searchPlaceholder?: string;
  searchValue?: string;
  onSearchChange?: (value: string) => void;
  onRetry?: () => void;
  actions?: (item: T) => ReactNode;
  // Pagination
  page?: number;
  pageSize?: number;
  totalCount?: number;
  onPageChange?: (page: number) => void;
}

// =============================================================================
// Component
// =============================================================================

export function DataTable<T>({
  columns,
  data,
  keyField,
  loading = false,
  error = null,
  emptyIcon,
  emptyTitle = "No data",
  emptyDescription = "No items found.",
  searchPlaceholder = "Search...",
  searchValue = "",
  onSearchChange,
  onRetry,
  actions,
  page = 1,
  pageSize = 10,
  totalCount,
  onPageChange,
}: DataTableProps<T>) {
  const totalPages = totalCount ? Math.ceil(totalCount / pageSize) : 1;
  const showPagination = totalCount && totalCount > pageSize;

  return (
    <div className="data-table-container">
      {/* Search Bar */}
      {onSearchChange && (
        <div className="data-table-toolbar">
          <div className="search-input-wrapper">
            <SearchIcon size={18} />
            <input
              type="text"
              placeholder={searchPlaceholder}
              value={searchValue}
              onChange={(e) => onSearchChange(e.target.value)}
              className="search-input"
            />
          </div>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="data-table-error">
          <span>{error}</span>
          {onRetry && <button onClick={onRetry}>Retry</button>}
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div className="data-table-loading">
          <div className="loading-spinner" />
          <span>Loading...</span>
        </div>
      )}

      {/* Empty State */}
      {!loading && !error && data.length === 0 && (
        <div className="data-table-empty">
          {emptyIcon}
          <h3>{emptyTitle}</h3>
          <p>{emptyDescription}</p>
        </div>
      )}

      {/* Table */}
      {!loading && !error && data.length > 0 && (
        <>
          <div className="data-table-wrapper">
            <table className="data-table">
              <thead>
                <tr>
                  {columns.map((col) => (
                    <th key={col.key} style={{ width: col.width }}>
                      {col.header}
                    </th>
                  ))}
                  {actions && <th style={{ width: "100px" }}>Actions</th>}
                </tr>
              </thead>
              <tbody>
                {data.map((item) => (
                  <tr key={String(item[keyField])}>
                    {columns.map((col) => (
                      <td key={col.key}>
                        {col.render
                          ? col.render(item)
                          : String((item as Record<string, unknown>)[col.key] ?? "")}
                      </td>
                    ))}
                    {actions && <td className="actions-cell">{actions(item)}</td>}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {showPagination && onPageChange && (
            <div className="data-table-pagination">
              <span className="pagination-info">
                Showing {(page - 1) * pageSize + 1} - {Math.min(page * pageSize, totalCount)} of {totalCount}
              </span>
              <div className="pagination-controls">
                <button
                  className="pagination-btn"
                  disabled={page <= 1}
                  onClick={() => onPageChange(page - 1)}
                >
                  <ChevronLeftIcon size={18} />
                </button>
                <span className="pagination-current">
                  Page {page} of {totalPages}
                </span>
                <button
                  className="pagination-btn"
                  disabled={page >= totalPages}
                  onClick={() => onPageChange(page + 1)}
                >
                  <ChevronRightIcon size={18} />
                </button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default DataTable;