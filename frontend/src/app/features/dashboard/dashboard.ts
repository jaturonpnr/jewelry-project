import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  imports: [MatCardModule, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h1>Dashboard</h1>
    <p class="welcome">
      Welcome back, <strong>{{ auth.user()?.fullName }}</strong>
    </p>

    <div class="cards">
      <mat-card>
        <mat-card-content>
          <div class="card-row">
            <mat-icon>business</mat-icon>
            <div>
              <div class="metric-label">Customers</div>
              <div class="metric-hint">View B2B customers and their orders</div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-content>
          <div class="card-row">
            <mat-icon>inventory_2</mat-icon>
            <div>
              <div class="metric-label">Inventory</div>
              <div class="metric-hint">Raw materials, stones, finished goods</div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-card>
        <mat-card-content>
          <div class="card-row">
            <mat-icon>receipt_long</mat-icon>
            <div>
              <div class="metric-label">Sales Orders</div>
              <div class="metric-hint">Track orders Draft → Delivered</div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    h1 { margin: 0 0 4px; font-weight: 500; }
    .welcome { color: #607d8b; margin: 0 0 24px; }
    .cards {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 16px;
    }
    .card-row {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    .card-row mat-icon {
      font-size: 40px;
      height: 40px;
      width: 40px;
      color: #3949ab;
    }
    .metric-label { font-size: 16px; font-weight: 500; }
    .metric-hint { color: #607d8b; font-size: 13px; }
  `],
})
export class Dashboard {
  protected readonly auth = inject(AuthService);
}
