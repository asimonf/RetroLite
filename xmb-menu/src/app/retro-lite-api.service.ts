import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface Game {
  Id: string;
  Name: string;
  System: string;
}

@Injectable({
  providedIn: 'root'
})
export class RetroLiteApiService {

  url = 'http://api.retrolite.internal/';
  games = null;

  constructor(private http: HttpClient) { }

  initialize() {
    return this.http.get(this.url + 'initialize').toPromise();
  }

  loadGames() {
    return this.http.get<Game[]>(this.url + 'games').toPromise().then((games: Game[]) => {
      this.games = games;
      console.dir(games[0].Id);
    });
  }
}
