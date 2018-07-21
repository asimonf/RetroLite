export class Menu {
  state: string;

  constructor() {
    this.state = 'active';
  }

  toggleState() {
    this.state = this.state === 'active' ? 'inactive' : 'active';
  }
}
