import { AddressDto } from '../customers/customer.types';

export const SalesOrderStatus = {
  Draft: 1,
  Confirmed: 2,
  InProduction: 3,
  Qc: 4,
  Packed: 5,
  Shipped: 6,
  Delivered: 7,
  Cancelled: 99,
} as const;

export type SalesOrderStatusValue = typeof SalesOrderStatus[keyof typeof SalesOrderStatus];

export type SalesOrderStatusName =
  | 'Draft'
  | 'Confirmed'
  | 'InProduction'
  | 'Qc'
  | 'Packed'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled';

export interface SalesOrderItem {
  id: string;
  lineNumber: number;
  description: string;
  designCode?: string | null;
  finishedGoodsId?: string | null;
  quantity: number;
  unitPrice: number;
  lineDiscount: number;
  lineTotal: number;
  notes?: string | null;
}

export interface SalesOrder {
  id: string;
  orderNumber: string;
  customerId: string;
  customerCode: string;
  customerName: string;
  currency: string;
  exchangeRateToBase?: number | null;
  status: SalesOrderStatusName;
  orderDate: string;
  requestedDeliveryDate?: string | null;
  confirmedAt?: string | null;
  shippedAt?: string | null;
  deliveredAt?: string | null;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  shippingCost: number;
  totalAmount: number;
  shippingAddress: AddressDto;
  trackingNumber?: string | null;
  notes?: string | null;
  items: SalesOrderItem[];
  rowVersion: string; // base64-encoded byte[]
  createdAt: string;
}

export interface ChangeSalesOrderStatusDto {
  newStatus: SalesOrderStatusValue;
  reason?: string | null;
  trackingNumber?: string | null;
  rowVersion: string;
}

/** Allowed transitions (mirrors backend SalesOrderStatusFlow per CLAUDE.md §10.9) */
const TRANSITIONS: Record<SalesOrderStatusName, SalesOrderStatusValue[]> = {
  Draft:        [SalesOrderStatus.Confirmed,    SalesOrderStatus.Cancelled],
  Confirmed:    [SalesOrderStatus.InProduction, SalesOrderStatus.Cancelled],
  InProduction: [SalesOrderStatus.Qc,           SalesOrderStatus.Cancelled],
  Qc:           [SalesOrderStatus.Packed,       SalesOrderStatus.Cancelled],
  Packed:       [SalesOrderStatus.Shipped,      SalesOrderStatus.Cancelled],
  Shipped:      [SalesOrderStatus.Delivered,    SalesOrderStatus.Cancelled],
  Delivered:    [],
  Cancelled:    [],
};

export function nextAllowedStatuses(current: SalesOrderStatusName): SalesOrderStatusValue[] {
  return TRANSITIONS[current] ?? [];
}

export const STATUS_LABELS: Record<SalesOrderStatusValue, string> = {
  [SalesOrderStatus.Draft]: 'Draft',
  [SalesOrderStatus.Confirmed]: 'Confirmed',
  [SalesOrderStatus.InProduction]: 'In Production',
  [SalesOrderStatus.Qc]: 'QC',
  [SalesOrderStatus.Packed]: 'Packed',
  [SalesOrderStatus.Shipped]: 'Shipped',
  [SalesOrderStatus.Delivered]: 'Delivered',
  [SalesOrderStatus.Cancelled]: 'Cancelled',
};

export const STATUS_ICONS: Record<SalesOrderStatusValue, string> = {
  [SalesOrderStatus.Draft]: 'edit_note',
  [SalesOrderStatus.Confirmed]: 'check_circle',
  [SalesOrderStatus.InProduction]: 'precision_manufacturing',
  [SalesOrderStatus.Qc]: 'fact_check',
  [SalesOrderStatus.Packed]: 'inventory_2',
  [SalesOrderStatus.Shipped]: 'local_shipping',
  [SalesOrderStatus.Delivered]: 'task_alt',
  [SalesOrderStatus.Cancelled]: 'cancel',
};
