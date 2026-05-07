// Mirrors backend DTOs

// ── Inventory Summary ────────────────────────────────────────────────

export interface InventorySummaryReport {
  totals: InventoryTotals;
  rawMaterialsByCategory: RawMaterialByCategory[];
  rawMaterialsByMaterial: RawMaterialByMaterial[];
  rawMaterialsByStatus: StockByStatus[];
  stoneItemsByStatus: StockByStatus[];
  stoneParcelsByStatus: StockByStatus[];
}

export interface InventoryTotals {
  rawMaterialLotCount: number;
  stoneItemCount: number;
  stoneParcelCount: number;
  totalPureGoldGrams: number;
  totalStoneCarat: number;
  totalStoneCount: number;
}

export interface RawMaterialByCategory {
  category: string;
  lotCount: number;
  totalQuantity: number;
}

export interface RawMaterialByMaterial {
  materialCode: string;
  materialName: string;
  unit: string;
  lotCount: number;
  totalQuantity: number;
  totalPureWeight: number | null;
}

export interface StockByStatus {
  status: string;
  count: number;
}

// ── Sales Order Pipeline ────────────────────────────────────────────

export interface SalesOrderPipelineReport {
  byStatus: OrdersByStatus[];
  topCustomers: OrdersByCustomer[];
  revenueByCurrency: OrdersByCurrency[];
  overdue: OverdueOrder[];
  totalOrders: number;
  openOrders: number;
}

export interface OrdersByStatus {
  status: string;
  count: number;
  totalAmount: number;
  currency: string | null;
}

export interface OrdersByCustomer {
  customerId: string;
  customerCode: string;
  customerName: string;
  orderCount: number;
  totalRevenue: number;
  currency: string;
}

export interface OrdersByCurrency {
  currency: string;
  orderCount: number;
  totalRevenue: number;
}

export interface OverdueOrder {
  orderId: string;
  orderNumber: string;
  customerName: string;
  status: string;
  requestedDeliveryDate: string;
  daysOverdue: number;
  currency: string;
  totalAmount: number;
}

// ── Production Load ─────────────────────────────────────────────────

export interface ProductionLoadReport {
  activeWorkOrders: number;
  completedThisMonth: number;
  cancelledThisMonth: number;
  totalLossGramsThisMonth: number;
  workOrdersByStatus: WorkOrdersByStatus[];
  workOrdersByPriority: WorkOrdersByPriority[];
  stagesInProgress: StagesByStage[];
  workerLoad: WorkerLoad[];
}

export interface WorkOrdersByStatus {
  status: string;
  count: number;
}

export interface WorkOrdersByPriority {
  priority: string;
  count: number;
}

export interface StagesByStage {
  stage: string;
  inProgressCount: number;
  pendingCount: number;
  completedCount: number;
  totalLossGrams: number | null;
}

export interface WorkerLoad {
  workerId: string;
  employeeCode: string;
  fullName: string;
  position: string;
  inProgressStages: number;
  pendingStages: number;
}
