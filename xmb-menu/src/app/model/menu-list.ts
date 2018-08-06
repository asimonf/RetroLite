import {MenuItem} from './menu-item';
import {Type} from '@angular/core';
import {Observable, Subscription} from 'rxjs';

export interface IMenuListComponent {
  menuList: MenuList;
}

export class MenuList {
  elementIndex: number;

  private subscription: Subscription = null;

  constructor(
    public readonly title: string,
    public readonly icon: string,
    public items: MenuItem[],
    public active: boolean,
    public readonly component: Type<any>
  ) {
    this.elementIndex = 0;
  }

  subscribeTo(observable: Observable<MenuItem[]>) {
    if (null !== this.subscription) {
      this.subscription.unsubscribe();
      this.subscription = null;
    }

    this.subscription = observable.subscribe((items) => {
      this.items = items;

      if (this.elementIndex >= items.length) {
        this.elementIndex = items.length - 1;
      }
    });
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

  currentItem(): MenuItem {
    return this.items[this.elementIndex];
  }

  select() {
    this.currentItem().select();
  }
}
