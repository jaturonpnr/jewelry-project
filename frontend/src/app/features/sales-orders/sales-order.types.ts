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
  createdAt: string;
}
