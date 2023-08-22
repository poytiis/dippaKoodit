import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-button',
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.scss']
})
export class ButtonComponent implements OnInit {

  constructor() { }

  @Input() text = ''
  @Input() type = ''
  @Input() id = ''
  @Output() click = new EventEmitter();

  ngOnInit(): void {
  }

}
