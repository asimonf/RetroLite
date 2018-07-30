import {Component, HostListener, OnInit} from '@angular/core';
import {Game, RetroLiteApiService} from '../retro-lite-api.service';
import {Observable} from 'rxjs';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {
  gameList$: Observable<Game[]>;
  lastGameList: Game[];

  constructor(private retroApi: RetroLiteApiService) {
    retroApi.getGames$().subscribe((val) => {
      this.lastGameList = val;
    });
  }

  ngOnInit() {
  }

  @HostListener('document:keyup', ['$event']) async onKeydownHandler(event: KeyboardEvent) {
    switch (event.key) {
      case 'd':
        await this.retroApi.loadGame(this.lastGameList[0].Id);
        break;
      case 'Escape':
        await this.retroApi.toggleState();
        break;
    }
  }

}
