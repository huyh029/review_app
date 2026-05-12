import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-evaluation-object',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './evaluation-object.component.html',
  styleUrls: ['./evaluation-object.component.css']
})
export class EvaluationObjectComponent implements OnInit {
  activeTab = 'configuration';

  constructor(private router: Router, private route: ActivatedRoute) {}

  ngOnInit() {
    // Set default tab
    this.activeTab = 'configuration';
  }

  selectTab(tab: string) {
    this.activeTab = tab;
    this.router.navigate([tab], { relativeTo: this.route });
  }
}
