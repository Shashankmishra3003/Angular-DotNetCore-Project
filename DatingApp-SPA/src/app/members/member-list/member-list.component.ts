import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  // grtitng user from local storage
  user: User = JSON.parse(localStorage.getItem('user'));
  // gender list for the options
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}];
  // object for storing the filter inputs
  userParams: any = {};
  pagination: Pagination;
  constructor(private userService: UserService, private alertify: AlertifyService,
              private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['users'].result;
      // getting the Pagination data
      this.pagination = data['users'].pagination;
    });

    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';
  }

  // Event to be fired when the pagination button is clicked
  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    // getitng the users from the Api for the new Page
    this.loadUser();
  }

  resetFilters(){
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.loadUser();
  }

  loadUser() {
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
        .subscribe((res: PaginatedResult<User[]>) => { // the return type is od PaginatedResult class
            this.users = res.result;
            this.pagination = res.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }

}
