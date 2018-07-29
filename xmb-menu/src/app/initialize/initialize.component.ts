import {Component, HostBinding, Input, OnInit} from '@angular/core';
import {RetroLiteApiService} from '../retro-lite-api.service';
import {Router} from '@angular/router';
import {animate, state, style, transition, trigger} from '@angular/animations';
import {Observable} from 'rxjs';
import { map, retry, catchError, delay } from 'rxjs/operators';
import {Menu} from '../model/menu';

@Component({
  selector: 'app-initialize',
  templateUrl: './initialize.component.html',
  styleUrls: ['./initialize.component.css'],
  animations: [
    trigger('state', [
      state('void',   style({
        opacity: 0
      })),
      state('in',   style({
        opacity: 1
      })),
      transition('void => *', [
        style({opacity: 0}),
        animate(1000)
      ]),
      transition('* => void', [
        animate(1000, style({opacity: 0}))
      ])
    ])
  ]
})
export class InitializeComponent implements OnInit {

  @HostBinding('@state') state;

  constructor(
    private retroApi: RetroLiteApiService,
    private router: Router
  ) {
    this.state = 'in';
  }

  ngOnInit() {
    Promise.all([
      this.retroApi.initialize().then(() => {
        this.retroApi.loadGames().then((data) => {
          this.router.navigate(['/menu']);
        });
      }),
    ]);
  }

}
