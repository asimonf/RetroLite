import {MenuList} from './menu-list';
import {MenuItem} from './menu-item';
import {MenuListComponent} from '../menu-list/menu-list.component';
import {Game, RetroLiteApiService} from '../retro-lite-api.service';

export class Menu {
  state: string;
  listIndex: number;
  readonly lists: MenuList[];

  private readonly mainMenuList: MenuList;

  constructor(private retroApi: RetroLiteApiService) {
    this.state = 'inactive';
    this.listIndex = 0;
    this.mainMenuList = new MenuList('RetroLite', 'settings', [
      new MenuItem('Scan Games', 'Search the games', 'info', true, () => {
        return retroApi.scanGameList();
      }),
      new MenuItem('Quit', 'Exit the frontend', 'info', false, () => {
        return retroApi.quit();
      }),
    ], true, MenuListComponent);

    this.lists = [
      this.mainMenuList
    ];

    retroApi.getGames$().subscribe((gameList: Game[]) => {
      const systemsList = [];
      const gameListPerSystem = {};
      this.lists.length = 0;
      this.lists.push(this.mainMenuList);

      gameList.forEach((game: Game) => {
        if (systemsList.lastIndexOf(game.System) < 0) {
          systemsList.push(game.System);
          gameListPerSystem[game.System] = [];
        }
        gameListPerSystem[game.System].push(
          new MenuItem(game.Name, game.Id, 'play_arrow', gameListPerSystem[game.System].length === 0, async () => {
            await this.retroApi.loadGame(game.Id);
            this.deactivate();
            return Promise.resolve();
          })
        );
      });

      systemsList.forEach((system: string) => {
        this.lists.push(new MenuList(system, 'info', gameListPerSystem[system], false, MenuListComponent));
      });
    });
  }

  async toggleState() {
    if (this.retroApi.isGameLoaded) {
      if (this.state === 'inactive') {
        await this.activate();
      } else {
        await this.deactivate();
      }
    }
  }

  async deactivate() {
    this.state = 'inactive';
    await this.retroApi.resume();
  }

  async activate() {
    this.state = 'active';
    await this.retroApi.pause();
  }

  isActive(): boolean {
    return this.state === 'active';
  }

  changeList(direction: number) {
    this.lists[this.listIndex].active = false;
    this.listIndex += direction;

    if (this.listIndex >= this.lists.length) {
      this.listIndex -= this.lists.length;
    } else if (this.listIndex < 0) {
      this.listIndex += this.lists.length;
    }
    this.lists[this.listIndex].active = true;
  }

  changeElement(offset: number) {
    this.currentList().changeElement(offset);
  }

  currentList(): MenuList {
    return this.lists[this.listIndex];
  }

  select() {
    this.currentList().select();
  }
}
