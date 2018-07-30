import {Component, HostBinding, HostListener} from '@angular/core';
import {Menu} from './model/menu';
import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';
import {RetroLiteApiService} from './retro-lite-api.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  animations: [
    trigger('state', [
      state('inactive', style({
        opacity: 0
      })),
      state('active',   style({
        opacity: 1
      })),
      transition('active <=> inactive', animate('1000ms ease'))
    ])
  ]
})
export class AppComponent {
  menu: Menu;

  constructor(private retroApi: RetroLiteApiService) {
    this.menu = retroApi.menu;
  }
}
