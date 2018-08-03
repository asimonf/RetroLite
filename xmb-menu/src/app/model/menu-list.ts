import {MenuElement} from './menu-element';

export class MenuList {
  readonly title: string;
  active: boolean;
  readonly icon: string;
  readonly items: MenuElement[];
  elementIndex: number;

  constructor(title: string, icon: string, items: MenuElement[], active: boolean) {
    this.title = title;
    this.icon = icon;
    this.items = items;
    this.active = active;
    this.elementIndex = 0;
  }

  changeElement(offset: number) {
    this.items[this.elementIndex].active = false;
    this.elementIndex += offset;

    if (this.elementIndex >= this.items.length) {
      this.elementIndex -= this.items.length;
    } else if (this.elementIndex < 0) {
      this.elementIndex += this.items.length;
    }
    this.items[this.elementIndex].active = true;
  }
}
