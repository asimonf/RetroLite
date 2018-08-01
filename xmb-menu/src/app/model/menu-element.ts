export class MenuElement {
  readonly title: string;
  readonly subtitle: string;
  active: boolean;
  readonly icon: string;


  constructor(title: string, subtitle: string, icon: string, active: boolean) {
    this.title = title;
    this.subtitle = subtitle;
    this.icon = icon;
    this.active = active;
  }
}
