import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./home/home.component').then(m => m.HomeComponent) },
  { path: 'invite/:token', loadComponent: () => import('./home/home.component').then(m => m.HomeComponent) },
  { path: 'details', loadComponent: () => import('./details/details.component').then(m => m.DetailsComponent) },
  { path: 'rsvp/:token', loadComponent: () => import('./rsvp/rsvp.component').then(m => m.RsvpComponent) },
  { path: 'rsvp', redirectTo: '' }, // no token → home
  { path: 'admin', loadComponent: () => import('./admin/admin.component').then(m => m.AdminComponent) },
  { path: '**', redirectTo: '' }
];
