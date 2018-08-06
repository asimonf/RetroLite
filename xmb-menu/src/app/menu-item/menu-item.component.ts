import {Component, Input, OnInit} from '@angular/core';
import {IMenuItem, MenuItem} from '../model/menu-item';

@Component({
  selector: 'app-menu-item',
  templateUrl: './menu-item.component.html',
  styleUrls: ['./menu-item.component.css']
})
export class MenuItemComponent implements OnInit, IMenuItem {

  @Input() menuItem: MenuItem;

  constructor() { }

  ngOnInit() {
  }

}
