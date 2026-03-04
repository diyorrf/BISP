import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Home, ArrowLeft } from 'lucide-angular';
import { Location } from '@angular/common';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './not-found.component.html'
})
export class NotFoundComponent {
  readonly icons = { Home, ArrowLeft };

  constructor(private location: Location) {}

  goBack(): void {
    this.location.back();
  }
}
