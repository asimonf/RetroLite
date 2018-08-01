import {Component, HostListener, OnInit} from '@angular/core';
import {Game, RetroLiteApiService} from '../retro-lite-api.service';
import {Menu} from '../model/menu';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {
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
}
