import {MenuList} from './menu-list';
import {MenuElement} from './menu-element';

export class Menu {
  state: string;
  listIndex: number;
  readonly lists: MenuList[];

  constructor() {
    this.state = 'inactive';
    this.listIndex = 0;

    this.lists = [
      new MenuList('settings', 'settings', [
        new MenuElement('info', 'subtitle', 'info', true),
        new MenuElement('grade', 'subtitle', 'grade', false),
        new MenuElement('toll', 'subtitle', 'toll', false),
        new MenuElement('grade', 'subtitle', 'grade', false),
        new MenuElement('toll', 'subtitle', 'toll', false),
      ], true),
      new MenuList('explore', 'explore', [
        new MenuElement('face', 'subtitle', 'face', true),
        new MenuElement('grade', 'subtitle', 'grade', false),
        new MenuElement('toll', 'subtitle', 'toll', false),
        new MenuElement('grade', 'subtitle', 'grade', false),
      ], false),
      new MenuList('play_arrow', 'play_arrow', [
        new MenuElement('face', 'subtitle', 'face', true),
        new MenuElement('explore', 'subtitle', 'explore', false),
      ], false),
    ];
  }

  deactivate() {
    this.state = 'inactive';
  }

  activate() {
    this.state = 'active';
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
}
