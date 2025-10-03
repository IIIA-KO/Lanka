import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
// Import standalone components
const OffersComponent = (): Promise<typeof import('./offers.component').OffersComponent> => import('./offers.component').then(m => m.OffersComponent);
const CreateOfferComponent = (): Promise<typeof import('./create-offer/create-offer.component').CreateOfferComponent> => import('./create-offer/create-offer.component').then(m => m.CreateOfferComponent);
const EditOfferComponent = (): Promise<typeof import('./edit-offer/edit-offer.component').EditOfferComponent> => import('./edit-offer/edit-offer.component').then(m => m.EditOfferComponent);
const OfferDetailsComponent = (): Promise<typeof import('./offer-details/offer-details.component').OfferDetailsComponent> => import('./offer-details/offer-details.component').then(m => m.OfferDetailsComponent);

const routes: Routes = [
  {
    path: '',
    loadComponent: OffersComponent,
  },
  {
    path: 'create',
    loadComponent: CreateOfferComponent,
  },
  {
    path: ':id',
    loadComponent: OfferDetailsComponent,
  },
  {
    path: ':id/edit',
    loadComponent: EditOfferComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OffersRoutingModule {}
