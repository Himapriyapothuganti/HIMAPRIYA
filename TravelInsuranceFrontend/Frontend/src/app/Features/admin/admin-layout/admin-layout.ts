import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../components/sidebar/sidebar';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, Sidebar],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout implements OnInit {
  adminName: string = 'Admin';

  ngOnInit(): void {
    const fullName = localStorage.getItem('fullName');
    if (fullName) {
      this.adminName = fullName;
    }
  }
}
