export class Menu {
  state: string;

  constructor() {
    this.state = 'active';
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
}
