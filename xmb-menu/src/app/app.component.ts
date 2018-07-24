import {Component, HostBinding, HostListener} from '@angular/core';
import {Menu} from './model/menu';
import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';

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
  title = 'app';

  menu: Menu;

  constructor() {
    this.menu = new Menu();
  }

  @HostListener('document:openmenu') openMenu() {
    this.menu.state = 'active';
  }

  @HostListener('document:closemenu') closeMenu() {
    this.menu.state = 'inactive';
  }
}
