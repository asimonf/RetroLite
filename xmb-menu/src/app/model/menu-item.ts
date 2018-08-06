import {Type} from '@angular/core';

export interface IMenuItem {
  menuItem: MenuItem;
}

export class MenuItem {
  constructor(
    public readonly title: string,
    public readonly subtitle: string,
    public readonly icon: string,
    public active: boolean,
    public readonly action: () => Promise<any>
  ) {
  }

  async select() {
    await this.action();
  }
}
