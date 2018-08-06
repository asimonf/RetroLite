import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {BehaviorSubject, Observable, Subject} from 'rxjs';
import {Menu} from './model/menu';

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

  private _gamesList: Game[] = [];
  private readonly _games$: BehaviorSubject<Game[]>;
  private _isGameRunning = false;
  private _isGameLoaded = false;

  constructor(private http: HttpClient) {
    this._games$ = new BehaviorSubject<Game[]>([]);
  }

  initialize() {
    return this.http.post(this.url + 'initialize', {}).toPromise();
  }

  getGames$() {
    return this._games$.asObservable();
  }

  get isGameLoaded(): boolean {
    return this._isGameLoaded;
  }

  async quit() {
    return this.http.post(this.url + 'quit', {}).toPromise();
  }

  async loadGame(id: string) {
    if (this._isGameLoaded) {
      return Promise.reject();
    }

    return this.http.post(this.url + 'games/' + id + '/load', null).toPromise().then(() => {
      this._isGameRunning = true;
      this._isGameLoaded = true;
    });
  }

  async pause() {
    if (!this._isGameLoaded) {
      return;
    }

    if (this._isGameRunning) {
      await this._pause();
      this._isGameRunning = false;
    }
  }

  async resume() {
    if (!this._isGameLoaded) {
      return;
    }

    if (!this._isGameRunning) {
      await this._resume();
      this._isGameRunning = true;
    }
  }

  refreshGameList() {
    return this.http.get<Game[]>(this.url + 'games').toPromise().then((games: Game[]) => {
      this._gamesList = games;
      this._games$.next(this._gamesList);
    });
  }

  private async _pause() {
    await this.http.get(this.url + 'event/open-menu').toPromise();
  }

  private async _resume() {
    await this.http.get(this.url + 'event/close-menu').toPromise();
  }
}
