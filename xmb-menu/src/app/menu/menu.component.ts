import {AfterViewInit, Component, HostListener, OnInit} from '@angular/core';
import {Game, RetroLiteApiService} from '../retro-lite-api.service';
import {Menu} from '../model/menu';
import {animate, state, style, transition, trigger} from '@angular/animations';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css'],
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
export class MenuComponent implements OnInit, AfterViewInit {
  lastGameList: Game[];
  menu: Menu;

  constructor(private retroApi: RetroLiteApiService) {
    this.menu = retroApi.menu;
    retroApi.getGames$().subscribe((val) => {
      this.lastGameList = val;
    });
  }

  ngOnInit() {

  }

  @HostListener('document:keyup', ['$event']) async onKeydownHandler(event: KeyboardEvent) {
    if (this.menu.isActive()) {
      switch (event.key) {
        case 'd':
          await this.retroApi.loadGame(this.lastGameList[3].Id);
          break;
        case 'Escape':
          await this.retroApi.toggleState();
          break;
      }
    } else {
      switch (event.key) {
        case 'Escape':
          await this.retroApi.toggleState();
          break;
      }
    }
  }

  ngAfterViewInit(): void {
    this.menu.activate();
  }
}
