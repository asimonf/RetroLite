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
})
export class AppComponent {
  menu: Menu;

  constructor(private retroApi: RetroLiteApiService) {
    this.menu = retroApi.menu;
  }
}
