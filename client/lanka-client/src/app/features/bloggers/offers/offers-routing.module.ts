import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Lazy load the unified form component
const OfferFormComponent = (): Promise<typeof import('./offer-form/offer-form.component').OfferFormComponent> => 
  import('./offer-form/offer-form.component').then(m => m.OfferFormComponent);

const OfferDetailsComponent = (): Promise<typeof import('./offer-details/offer-details.component').OfferDetailsComponent> => 
  import('./offer-details/offer-details.component').then(m => m.OfferDetailsComponent);

const routes: Routes = [
  {
    path: '',
    redirectTo: '/pact', // List view removed, redirect to Pact page which has the list sidebar
    pathMatch: 'full'
  },
  {
    path: 'create',
    loadComponent: OfferFormComponent,
  },
  {
    path: ':id',
    loadComponent: OfferDetailsComponent,
  },
  {
    path: ':id/edit',
    loadComponent: OfferFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OffersRoutingModule {}
