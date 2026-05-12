export const WorkOrderStatus = {
  Draft: 1,
  Released: 2,
  InProgress: 3,
  OnHold: 4,
  Completed: 5,
  Cancelled: 99,
} as const;

export type WorkOrderStatusValue = typeof WorkOrderStatus[keyof typeof WorkOrderStatus];

export type WorkOrderStatusName =
  | 'Draft' | 'Released' | 'InProgress' | 'OnHold' | 'Completed' | 'Cancelled';

export const WorkOrderPriority = {
  Low: 1, Normal: 2, High: 3, Urgent: 4,
} as const;

export type WorkOrderPriorityValue = typeof WorkOrderPriority[keyof typeof WorkOrderPriority];

export const ProductionStage = {
  WaxModel: 1, Casting: 2, FilingCleaning: 3, StoneSetting: 4,
  Polishing: 5, Plating: 6, Assembly: 7, FinalQc: 8, Packaging: 9,
} as const;

export type WorkOrderStageStatusName =
  | 'Pending' | 'InProgress' | 'Completed' | 'Skipped' | 'Failed';

export interface WorkOrderStage {
  id: string;
  sequenceNumber: number;
  stage: string;            // 'WaxModel' | ...
  status: WorkOrderStageStatusName;
  assignedWorkerId: string | null;
  assignedWorkerName: string | null;
  estimatedHours: number;
  actualHours: number | null;
  startedAt: string | null;
  completedAt: string | null;
  weightInGrams: number | null;
  weightOutGrams: number | null;
  lossGrams: number | null;
  notes: string | null;
  failureReason: string | null;
  rowVersion: string;
}

export interface WorkOrder {
  id: string;
  workOrderNumber: string;
  salesOrderId: string | null;
  salesOrderNumber: string | null;
  designCode: string | null;
  description: string;
  quantity: number;
  status: WorkOrderStatusName;
  priority: string;
  scheduledStart: string | null;
  scheduledEnd: string | null;
  actualStart: string | null;
  actualEnd: string | null;
  releasedAt: string | null;
  completedAt: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  assignedSupervisorId: string | null;
  assignedSupervisorName: string | null;
  notes: string | null;
  stages: WorkOrderStage[];
  rowVersion: string;
  createdAt: string;
}

// ── Command DTOs ────────────────────────────────────────────────────

export interface CreateStageEstimateDto {
  stage: number;
  estimatedHours: number;
  assignedWorkerId?: string | null;
}

export interface CreateWorkOrderDto {
  workOrderNumber: string;
  salesOrderId?: string | null;
  designCode?: string | null;
  description: string;
  quantity: number;
  priority: WorkOrderPriorityValue;
  scheduledStart?: string | null;
  scheduledEnd?: string | null;
  assignedSupervisorId?: string | null;
  notes?: string | null;
  stageEstimates?: CreateStageEstimateDto[];
}

export interface ChangeWorkOrderStatusDto {
  newStatus: WorkOrderStatusValue;
  reason?: string | null;
  rowVersion: string;
}

export interface StartStageDto {
  assignedWorkerId?: string | null;
  weightInGrams?: number | null;
  rowVersion: string;
}

export interface CompleteStageDto {
  weightOutGrams?: number | null;
  actualHours: number;
  notes?: string | null;
  rowVersion: string;
}

export interface SkipStageDto {
  reason: string;
  rowVersion: string;
}

export interface FailStageDto {
  failureReason: string;
  weightOutGrams?: number | null;
  rowVersion: string;
}

// ── Status flow (mirror backend) ────────────────────────────────────

const TRANSITIONS: Record<WorkOrderStatusName, WorkOrderStatusValue[]> = {
  Draft:      [WorkOrderStatus.Released,    WorkOrderStatus.Cancelled],
  Released:   [WorkOrderStatus.InProgress,  WorkOrderStatus.Cancelled],
  InProgress: [WorkOrderStatus.OnHold,      WorkOrderStatus.Completed, WorkOrderStatus.Cancelled],
  OnHold:     [WorkOrderStatus.InProgress,  WorkOrderStatus.Cancelled],
  Completed:  [],
  Cancelled:  [],
};

export function nextAllowedStatuses(current: WorkOrderStatusName): WorkOrderStatusValue[] {
  return TRANSITIONS[current] ?? [];
}

export const STATUS_LABELS: Record<WorkOrderStatusValue, string> = {
  [WorkOrderStatus.Draft]: 'Draft',
  [WorkOrderStatus.Released]: 'Released',
  [WorkOrderStatus.InProgress]: 'In Progress',
  [WorkOrderStatus.OnHold]: 'On Hold',
  [WorkOrderStatus.Completed]: 'Completed',
  [WorkOrderStatus.Cancelled]: 'Cancelled',
};

export const STAGE_PRETTY: Record<string, string> = {
  WaxModel: 'Wax Model',
  Casting: 'Casting',
  FilingCleaning: 'Filing/Cleaning',
  StoneSetting: 'Stone Setting',
  Polishing: 'Polishing',
  Plating: 'Plating',
  Assembly: 'Assembly',
  FinalQc: 'Final QC',
  Packaging: 'Packaging',
};

export const STAGE_ICONS: Record<string, string> = {
  WaxModel: 'edit_note',
  Casting: 'whatshot',
  FilingCleaning: 'cleaning_services',
  StoneSetting: 'diamond',
  Polishing: 'auto_fix_high',
  Plating: 'shower',
  Assembly: 'construction',
  FinalQc: 'fact_check',
  Packaging: 'inventory_2',
};
