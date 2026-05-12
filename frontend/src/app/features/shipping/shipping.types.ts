// ── Enums ────────────────────────────────────────────────────────────────────

export const ShipmentStatus = {
  Preparing: 1, Shipped: 2, InTransit: 3, Delivered: 4, Returned: 5,
} as const;
export type ShipmentStatusValue = typeof ShipmentStatus[keyof typeof ShipmentStatus];
export const SHIPMENT_STATUS_LABELS: Record<ShipmentStatusValue, string> = {
  1: 'Preparing', 2: 'Shipped', 3: 'In Transit', 4: 'Delivered', 5: 'Returned',
};

export const InvoiceStatus = {
  Draft: 1, Sent: 2, Paid: 3, Overdue: 4, Cancelled: 99,
} as const;
export type InvoiceStatusValue = typeof InvoiceStatus[keyof typeof InvoiceStatus];
export const INVOICE_STATUS_LABELS: Record<InvoiceStatusValue, string> = {
  1: 'Draft', 2: 'Sent', 3: 'Paid', 4: 'Overdue', 99: 'Cancelled',
};

export const INVOICE_STATUS_TRANSITIONS: Record<InvoiceStatusValue, InvoiceStatusValue[]> = {
  1: [2, 99], 2: [3, 4, 99], 3: [], 4: [3, 99], 99: [],
};

export const Currency = {
  THB: 1, USD: 2, EUR: 3, GBP: 4, AUD: 5, NZD: 6, SGD: 7, JPY: 8,
} as const;
export type CurrencyValue = typeof Currency[keyof typeof Currency];
export const CURRENCY_CODES: Record<CurrencyValue, string> = {
  1: 'THB', 2: 'USD', 3: 'EUR', 4: 'GBP', 5: 'AUD', 6: 'NZD', 7: 'SGD', 8: 'JPY',
};

// ── Shipment types ────────────────────────────────────────────────────────────

export interface ShipmentItemResponse {
  id: string;
  description: string;
  designCode: string | null;
  quantity: number;
  weightGrams: number;
  unitValueUsd: number | null;
  workOrderId: string | null;
  workOrderNumber: string | null;
}

export interface ShipmentResponse {
  id: string;
  shipmentNumber: string;
  salesOrderId: string | null;
  salesOrderNumber: string | null;
  status: ShipmentStatusValue;
  shipDate: string | null;
  estimatedDelivery: string | null;
  actualDelivery: string | null;
  carrier: string | null;
  trackingNumber: string | null;
  shippingMethod: string | null;
  totalWeightGrams: number;
  packingNotes: string | null;
  items: ShipmentItemResponse[];
  createdAt: string;
}

export interface ShipmentSummary {
  id: string;
  shipmentNumber: string;
  salesOrderNumber: string | null;
  status: ShipmentStatusValue;
  shipDate: string | null;
  carrier: string | null;
  trackingNumber: string | null;
  itemCount: number;
  totalWeightGrams: number;
  createdAt: string;
}

export interface ShipmentItemDto {
  description: string;
  designCode?: string | null;
  quantity: number;
  weightGrams: number;
  unitValueUsd?: number | null;
  workOrderId?: string | null;
}

export interface CreateShipmentDto {
  salesOrderId?: string | null;
  shipDate?: string | null;
  estimatedDelivery?: string | null;
  carrier?: string | null;
  trackingNumber?: string | null;
  shippingMethod?: string | null;
  packingNotes?: string | null;
  items: ShipmentItemDto[];
}

export interface UpdateShipmentStatusDto {
  newStatus: ShipmentStatusValue;
  actualDelivery?: string | null;
  trackingNumber?: string | null;
}

// ── Invoice types ─────────────────────────────────────────────────────────────

export interface InvoiceLineItemResponse {
  id: string;
  lineNumber: number;
  description: string;
  designCode: string | null;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  lineTotal: number;
}

export interface InvoiceResponse {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  salesOrderId: string | null;
  salesOrderNumber: string | null;
  shipmentId: string | null;
  shipmentNumber: string | null;
  status: InvoiceStatusValue;
  invoiceDate: string;
  dueDate: string;
  currency: CurrencyValue;
  currencyCode: string;
  exchangeRateToThb: number;
  vatApplicable: boolean;
  vatPercent: number;
  paymentTerms: string | null;
  notes: string | null;
  paidAt: string | null;
  paymentReference: string | null;
  subtotal: number;
  discountAmount: number;
  vatAmount: number;
  totalAmount: number;
  totalAmountThb: number;
  lineItems: InvoiceLineItemResponse[];
  createdAt: string;
}

export interface InvoiceSummary {
  id: string;
  invoiceNumber: string;
  customerName: string;
  salesOrderNumber: string | null;
  status: InvoiceStatusValue;
  invoiceDate: string;
  dueDate: string;
  currency: CurrencyValue;
  totalAmount: number;
  totalAmountThb: number;
  createdAt: string;
}

export interface InvoiceLineItemDto {
  lineNumber: number;
  description: string;
  designCode?: string | null;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
}

export interface CreateInvoiceDto {
  customerId: string;
  salesOrderId?: string | null;
  shipmentId?: string | null;
  invoiceDate: string;
  dueDate: string;
  currency: CurrencyValue;
  exchangeRateToThb: number;
  vatApplicable: boolean;
  paymentTerms?: string | null;
  notes?: string | null;
  lineItems: InvoiceLineItemDto[];
}

export interface UpdateInvoiceStatusDto {
  newStatus: InvoiceStatusValue;
  paidAt?: string | null;
  paymentReference?: string | null;
}
