import {Component, Input, OnInit} from '@angular/core';
import {IMenuListComponent, MenuList} from '../model/menu-list';

@Component({
  selector: 'app-menu-list',
  templateUrl: './menu-list.component.html',
  styleUrls: ['./menu-list.component.css']
})
export class MenuListComponent implements OnInit, IMenuListComponent {

  @Input() menuList: MenuList;

  constructor() { }

  ngOnInit() {
  }

}
