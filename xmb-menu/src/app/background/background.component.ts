import {Component, ElementRef, AfterViewInit} from '@angular/core';

@Component({
  selector: 'app-background',
  templateUrl: './background.component.html',
  styleUrls: ['./background.component.css']
})
export class BackgroundComponent implements AfterViewInit {
  constructor(private elRef: ElementRef) {
  }

  ngAfterViewInit(): void {
    this.reInit();
  }

  reInit(): void {

  }
}
