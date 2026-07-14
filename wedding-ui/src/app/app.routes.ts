import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./home/home.component').then(m => m.HomeComponent) },
  { path: 'invite/:token', loadComponent: () => import('./home/home.component').then(m => m.HomeComponent) },
  { path: 'entourage', loadComponent: () => import('./entourage/entourage.component').then(m => m.EntourageComponent) },
  { path: 'directions', loadComponent: () => import('./details/details.component').then(m => m.DetailsComponent) },
  { path: 'attire', loadComponent: () => import('./attire/attire.component').then(m => m.AttireComponent) },
  { path: 'gifts', loadComponent: () => import('./gifts/gifts.component').then(m => m.GiftsComponent) },
  { path: 'rsvp/:token', loadComponent: () => import('./rsvp/rsvp.component').then(m => m.RsvpComponent) },
  { path: 'rsvp', loadComponent: () => import('./rsvp/rsvp.component').then(m => m.RsvpComponent) },
  { path: 'admin', loadComponent: () => import('./admin/admin.component').then(m => m.AdminComponent) },
  { path: '**', redirectTo: '' }
];
