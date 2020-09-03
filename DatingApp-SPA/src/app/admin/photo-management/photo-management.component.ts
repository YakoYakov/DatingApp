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

}
