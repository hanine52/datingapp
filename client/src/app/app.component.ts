import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Spinkit } from 'ng-http-loader';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  public spinkit = Spinkit;
  title = 'The dating app';
  users: any;
  constructor(private accountService: AccountService, private presence: PresenceService) {

  }

  ngOnInit() {
    this.setCurrentUser();
  }

  setCurrentUser() {
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (user) {
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);

    }
    
  }

}
