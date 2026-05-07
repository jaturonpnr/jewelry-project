import { CurrencyValue } from '../customers/customer.types';

// ── Raw Material ─────────────────────────────────────────

export interface RawMaterialItem {
  id: string;
  materialTypeId: string;
  materialTypeCode: string;
  materialTypeName: string;
  materialUnit: string;
  lotNumber: string;
  location: string;
  quantity: number;
  pureWeight?: number | null;
  unitCost: number;
  costCurrency: string;
  supplierId?: string | null;
  supplierName?: string | null;
  receivedDate: string;
  status: string;
  notes?: string | null;
  rowVersion: string;
  createdAt: string;
}

export interface ReceiveRawMaterialDto {
  materialTypeId: string;
  lotNumber: string;
  location: string;
  quantity: number;
  unitCost: number;
  costCurrency: CurrencyValue;
  supplierId?: string | null;
  receivedDate: string;
  notes?: string | null;
}

export interface AdjustRawMaterialDto {
  quantityDelta: number;
  reason: string;
  notes?: string | null;
  rowVersion: string;
}

// ── Stone Item ───────────────────────────────────────────

export const StoneShape = {
  Round: 1, Princess: 2, Cushion: 3, Oval: 4, Emerald: 5, Marquise: 6,
  Pear: 7, Heart: 8, Asscher: 9, Radiant: 10, Baguette: 11, Other: 99,
} as const;

export const StoneCertAuthority = {
  None: 0, GIA: 1, IGI: 2, AGS: 3, HRD: 4, GUBELIN: 5, SSEF: 6, Other: 99,
} as const;

export interface StoneItem {
  id: string;
  itemCode: string;
  materialTypeId: string;
  materialTypeName: string;
  caratWeight: number;
  shape: string;
  color?: string | null;
  clarity?: string | null;
  cut?: string | null;
  measurements?: string | null;
  certAuthority: string;
  certificateNumber?: string | null;
  origin?: string | null;
  supplierId?: string | null;
  supplierName?: string | null;
  receivedDate: string;
  location: string;
  unitCost: number;
  costCurrency: string;
  status: string;
  notes?: string | null;
  rowVersion: string;
  createdAt: string;
}

// ── Stone Parcel ─────────────────────────────────────────

export interface StoneParcel {
  id: string;
  parcelCode: string;
  materialTypeId: string;
  materialTypeName: string;
  totalCarat: number;
  stoneCount: number;
  averageSize: number;
  qualityGrade?: string | null;
  shape: string;
  supplierId?: string | null;
  supplierName?: string | null;
  receivedDate: string;
  location: string;
  unitCostPerCarat: number;
  costCurrency: string;
  status: string;
  notes?: string | null;
  rowVersion: string;
  createdAt: string;
}

// ── Stock Movement ──────────────────────────────────────

export interface StockMovement {
  id: string;
  itemType: 'RawMaterial' | 'StoneItem' | 'StoneParcel' | 'FinishedGoods';
  itemId: string;
  movementType: string;
  quantityDelta: number;
  quantityAfter: number;
  referenceType?: string | null;
  referenceId?: string | null;
  reason?: string | null;
  notes?: string | null;
  performedBy: string;
  performedAt: string;
}
