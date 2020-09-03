import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AuthService } from 'src/app/_services/auth.service';
import { AdminService } from 'src/app/_services/admin.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.scss']
})
export class PhotoManagementComponent implements OnInit {
  photosForModeration: Photo[];

  constructor(
    private authService: AuthService,
    private adminService: AdminService,
    private alertify: AlertifyService) { }

  ngOnInit() {
    this.getPhotosForModeration();
  }

  getPhotosForModeration() {
    this.adminService.getPhotosForModeration().subscribe(photos => {
      this.photosForModeration = photos as Photo[];
    }, error => {
      this.alertify.error(error);
    });
  }

  disapprovePhoto(id: number) {
    this.alertify.confirm('This action will delete permenently the photo', () => {
      this.adminService.disapprovePhoto(id).subscribe(() => {
        this.photosForModeration.splice(this.photosForModeration.findIndex(p => p.id === id), 1);
        this.alertify.success('Photo was disapproved');
      }, () => {
        this.alertify.error('Failed to mark photo as inappropriate');
      });
    }, 'Mark Photo as inappropriate');
  }
}
