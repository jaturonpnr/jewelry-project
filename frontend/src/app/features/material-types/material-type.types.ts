export const MaterialCategory = {
  Metal: 1,
  PreciousStone: 2,
  SemiPreciousStone: 3,
  Finding: 4,
  Consumable: 5,
} as const;

export const MaterialUnit = {
  Gram: 1,
  Carat: 2,
  Piece: 3,
  Liter: 4,
  Kilogram: 5,
} as const;

export interface MaterialType {
  id: string;
  code: string;
  name: string;
  category: string;          // backend returns enum name as string
  unit: string;
  purityFraction?: number | null;
  karat?: number | null;
  isActive: boolean;
  description?: string | null;
  createdAt: string;
}
