import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { SelfEvaluationComponent } from './self-evaluation/self-evaluation.component';
import { ManagerEvaluationComponent } from './manager-evaluation/manager-evaluation.component';
import { ResultEvaluationComponent } from './result-evaluation/result-evaluation.component';

@Component({
  selector: 'app-evaluation-board',
  imports: [CommonModule, RouterModule, SelfEvaluationComponent, ManagerEvaluationComponent, ResultEvaluationComponent],
  templateUrl: './evaluation-board.component.html',
  styleUrls: ['./evaluation-board.component.css']
})
export class EvaluationBoardComponent {
  tabs = [
    { label: 'Tự đánh giá', type: 'self' },
    { label: 'Đánh giá', type: 'manager' },
    { label: 'Kết quả đánh giá', type: 'result' }
  ];
  
  activeTab: string = 'self';

  constructor(private route: ActivatedRoute, private router: Router) {
    this.route.params.subscribe(params => {
      if (params['type']) {
        this.activeTab = params['type'];
      }
    });
  }

  selectTab(type: string) {
    this.router.navigate(['/evaluation-board', type]);
  }
}



