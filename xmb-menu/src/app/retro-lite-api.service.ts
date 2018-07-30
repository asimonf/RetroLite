import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {BehaviorSubject, Observable, Subject} from 'rxjs';
import {Menu} from './model/menu';
import {AnonymousSubject} from 'rxjs/internal-compatibility';

export interface Game {
  Id: string;
  Name: string;
  System: string;
}

@Injectable({
  providedIn: 'root'
})
export class RetroLiteApiService {

  private url = 'http://api.retrolite.internal/';

  private gamesList: Game[] = [];
  private readonly games$: BehaviorSubject<Game[]>;
  readonly menu: Menu = new Menu();
  private isGameRunning = false;
  private isGameLoaded = false;

  constructor(private http: HttpClient) {
    this.games$ = new BehaviorSubject<Game[]>([]);
  }

  initialize() {
    return this.http.get(this.url + 'initialize').toPromise();
  }

  getGames$() {
    return this.games$.asObservable();
  }

  async loadGame(id: string) {
    if (this.isGameLoaded) {
      return Promise.reject();
    }

    return this.http.post(this.url + 'games/' + id + '/load', null).toPromise().then(() => {
      this.isGameRunning = true;
      this.isGameLoaded = true;
      this.menu.deactivate();
    });
  }

  async toggleState() {
    if (!this.isGameLoaded) { return; }

    if (this.isGameRunning) {
      await this._pause();
      this.menu.activate();
      this.isGameRunning = false;
    } else {
      await this._resume();
      this.menu.deactivate();
      this.isGameRunning = true;
    }
  }

  refreshGameList() {
    return this.http.get<Game[]>(this.url + 'games').toPromise().then((games: Game[]) => {
      this.gamesList = games;
      this.games$.next(this.gamesList);
    });
  }

  private async _pause() {
    await this.http.get(this.url + 'event/open-menu').toPromise();
  }

  private async _resume() {
    await this.http.get(this.url + 'event/close-menu').toPromise();
  }
}
