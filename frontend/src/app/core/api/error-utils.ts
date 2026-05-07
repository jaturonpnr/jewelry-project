import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from './api-response';

/**
 * Best-effort extraction of a user-facing message from any error
 * the backend or HTTP layer may throw.
 */
export function extractErrorMessage(err: unknown, fallback = 'Something went wrong.'): string {
  if (Array.isArray(err)) {
    return (err as ApiError[]).map((e) => e.message).join(', ');
  }

  if (err instanceof HttpErrorResponse) {
    const errors = err.error?.errors as ApiError[] | undefined;
    if (errors?.length) return errors.map((e) => e.message).join(', ');
    if (err.message) return err.message;
  }

  if (typeof err === 'string') return err;
  return fallback;
}

/**
 * Per-field map of validation errors (when backend returns 400).
 * Used to surface inline messages on form controls.
 */
export function extractFieldErrors(err: unknown): Record<string, string> {
  const map: Record<string, string> = {};
  if (err instanceof HttpErrorResponse) {
    const errors = err.error?.errors as ApiError[] | undefined;
    errors?.forEach((e) => {
      // backend uses "Request.Email" → strip "Request." prefix and lowercase first letter
      const field = e.field.replace(/^Request\./, '');
      const key = field.charAt(0).toLowerCase() + field.slice(1);
      map[key] = e.message;
    });
  }
  return map;
}
