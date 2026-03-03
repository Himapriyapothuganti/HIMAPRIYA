import { Routes } from '@angular/router';
import { LandingPage } from './Features/landing-page/landing-page';
import { LoginComponent } from './Features/auth/login/login.component';
import { RegisterComponent } from './Features/auth/register/register.component';
import { authGuard } from './Core/Guards/auth.guard';
import { AdminLayout } from './Features/admin/admin-layout/admin-layout';
import { Dashboard } from './Features/admin/dashboard/dashboard';
import { Users } from './Features/admin/users/users';
import { PolicyProducts } from './Features/admin/policy-products/policy-products';
import { Policies } from './Features/admin/policies/policies';
import { Claims } from './Features/admin/claims/claims';
import { CustomerLayoutComponent } from './Features/customer/customer-layout/customer-layout.component';
import { Dashboard as CustomerDashboard } from './Features/customer/dashboard/dashboard';
import { BrowsePlans } from './Features/customer/browse-plans/browse-plans';
import { PurchasePolicy } from './Features/customer/purchase-policy/purchase-policy';
import { Payment } from './Features/customer/payment/payment';
import { MyPolicies } from './Features/customer/my-policies/my-policies';
import { MyClaims } from './Features/customer/my-claims/my-claims';
import { OfficerLayoutComponent } from './Features/claims-officer/officer-layout.component';
import { OfficerDashboardComponent } from './Features/claims-officer/dashboard/officer-dashboard.component';
import { OfficerClaimsComponent } from './Features/claims-officer/claims/officer-claims.component';
import { OfficerClaimDetailComponent } from './Features/claims-officer/claim-detail/officer-claim-detail.component';

export const routes: Routes = [
  {
    path: '',
    component: LandingPage
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'admin',
    component: AdminLayout,
    canActivate: [authGuard],
    data: { role: 'Admin' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'users', component: Users },
      { path: 'policy-products', component: PolicyProducts },
      { path: 'policies', component: Policies },
      { path: 'claims', component: Claims }
    ]
  },
  {
    path: 'customer',
    component: CustomerLayoutComponent,
    canActivate: [authGuard],
    data: { role: 'Customer' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: CustomerDashboard },
      { path: 'browse-plans', component: BrowsePlans },
      { path: 'purchase/:productId', component: PurchasePolicy },
      { path: 'payment/:policyId', component: Payment },
      { path: 'my-policies', component: MyPolicies },
      { path: 'my-claims', component: MyClaims }
    ]
  },
  {
    path: 'agent',
    loadComponent: () => import('./Features/agent/agent-layout/agent-layout').then(m => m.AgentLayout),
    canActivate: [authGuard],
    data: { role: 'Agent' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./Features/agent/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'my-policies', loadComponent: () => import('./Features/agent/my-policies/my-policies').then(m => m.MyPolicies) },
      { path: 'my-policies/:id', loadComponent: () => import('./Features/agent/policy-detail/policy-detail').then(m => m.PolicyDetail) }
    ]
  },
  {
    path: 'officer',
    component: OfficerLayoutComponent,
    canActivate: [authGuard],
    data: { role: 'ClaimsOfficer' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: OfficerDashboardComponent },
      { path: 'claims', component: OfficerClaimsComponent },
      { path: 'claims/:id', component: OfficerClaimDetailComponent }
    ]
  }
];
