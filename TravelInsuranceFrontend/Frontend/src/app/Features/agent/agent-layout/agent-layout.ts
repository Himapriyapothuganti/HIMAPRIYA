import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet, ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AgentSidebar } from '../components/agent-sidebar/agent-sidebar.component';
import { AgentNavbar } from '../components/agent-navbar/agent-navbar.component';

@Component({
    selector: 'app-agent-layout',
    standalone: true,
    imports: [CommonModule, RouterModule, AgentSidebar, AgentNavbar],
    template: `
    <div class="flex min-h-screen font-sans selection:bg-[#E8584A]/20" style="background: #FDF4F0;">
      <!-- Fixed Sidebar -->
      <app-agent-sidebar></app-agent-sidebar>

      <!-- Main Content Area -->
      <div class="flex-1 ml-[260px] flex flex-col min-h-screen">
        <!-- Sticky Navbar -->
        <app-agent-navbar [title]="pageTitle"></app-agent-navbar>

        <!-- Dynamic Page Inject -->
        <main class="flex-1 p-8">
          <div class="max-w-7xl mx-auto min-h-[500px]">
             <router-outlet></router-outlet>
          </div>
        </main>
      </div>
    </div>
  `
})
export class AgentLayout {
    pageTitle: string = 'Agent Dashboard';

    constructor(private router: Router) {
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd)
        ).subscribe((event: any) => {
            this.updateTitleByUrl(event.urlAfterRedirects);
        });
        this.updateTitleByUrl(this.router.url);
    }

    private updateTitleByUrl(url: string) {
        if (url.includes('dashboard')) this.pageTitle = 'Dashboard';
        else if (url.includes('my-policies')) {
            // Basic check to see if it's the detail view
            if (url.split('/').length > 3) {
                this.pageTitle = 'Policy Details';
            } else {
                this.pageTitle = 'Assigned Policies';
            }
        }
        else this.pageTitle = 'Agent Portal';
    }
}
