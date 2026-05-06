import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="auth-shell">
      <div class="auth-card">
        <div class="brand">
          <span class="material-icons brand-icon">diamond</span>
          <h1>JewelryFactory</h1>
          <p>Manufacturing & Distribution ERP</p>
        </div>
        <router-outlet />
      </div>
    </div>
  `,
  styles: [`
    .auth-shell {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #1a237e 0%, #283593 60%, #3949ab 100%);
      padding: 24px;
    }
    .auth-card {
      width: 100%;
      max-width: 420px;
      background: #fff;
      border-radius: 12px;
      padding: 40px 32px;
      box-shadow: 0 12px 32px rgba(0, 0, 0, 0.18);
    }
    .brand { text-align: center; margin-bottom: 24px; }
    .brand-icon { font-size: 56px; color: #3949ab; }
    .brand h1 { margin: 8px 0 4px; font-weight: 500; font-size: 28px; }
    .brand p { margin: 0; color: #607d8b; font-size: 13px; }
  `],
})
export class AuthLayout {}
