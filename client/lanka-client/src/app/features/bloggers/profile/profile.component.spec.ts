import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { ProfileComponent } from './profile.component';
import { AgentService } from '../../../core/api/agent';
import { InstagramStatusService } from '../../../core/services/instagram-status.service';

const profileMock = {
  id: '11111111-1111-1111-1111-111111111111',
  firstName: 'Jane',
  lastName: 'Doe',
  email: 'jane@example.com',
  birthDate: '1995-01-01T00:00:00Z',
  bio: 'Content creator',
  profilePhotoUri: '',
  instagramUsername: 'janedoe',
  instagramFollowersCount: 2500,
  instagramMediaCount: 120,
};

class AgentServiceStub {
  public Analytics = {
    getAgeDistribution: () => of({ agePercentages: [] }),
    getGenderDistribution: () => of({ genderPercentages: [] }),
    getLocationDistribution: () => of({ locationPercentages: [] }),
  };
}

class InstagramStatusServiceStub {
  public linkingStatus$ = of(null);
  public renewalStatus$ = of(null);
  public init(): void {
    return;
  }
}

describe('ProfileComponent', () => {
  let component: ProfileComponent;
  let fixture: ComponentFixture<ProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { data: { profile: profileMock } } } },
        { provide: AgentService, useClass: AgentServiceStub },
        { provide: InstagramStatusService, useClass: InstagramStatusServiceStub },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
