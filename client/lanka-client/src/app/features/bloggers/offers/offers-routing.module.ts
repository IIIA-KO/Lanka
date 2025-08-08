import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
// Import standalone components
const OffersComponent = () => import('./offers.component').then(m => m.OffersComponent);
const CreateOfferComponent = () => import('./create-offer/create-offer.component').then(m => m.CreateOfferComponent);
const EditOfferComponent = () => import('./edit-offer/edit-offer.component').then(m => m.EditOfferComponent);
const OfferDetailsComponent = () => import('./offer-details/offer-details.component').then(m => m.OfferDetailsComponent);

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
