import { Routes } from '@angular/router';
import { LoginComponent } from './page/auth/login/login.component';
import { CallbackComponent } from './page/auth/callback/callback.component';
import { HomeComponent } from './page/protected/home/home.component';
import { EvaluationBoardComponent } from './page/protected/evaluation-board/evaluation-board.component';
import { ReportsComponent } from './page/protected/reports/reports.component';
import { EvaluationObjectComponent } from './page/protected/configuration/evaluation-object/evaluation-object.component';
import { ConfigurationComponent } from './page/protected/configuration/evaluation-object/configuration/configuration.component';
import { RoleComponent } from './page/protected/configuration/evaluation-object/role/role.component';
import { EvaluationFlowComponent } from './page/protected/configuration/evaluation-flow/evaluation-flow.component';
import { EvaluationFlowDetailComponent } from './page/protected/configuration/evaluation-flow/detail/evaluation-flow-detail.component';
import { EvaluationCriteriaComponent } from './page/protected/configuration/evaluation-criteria/evaluation-criteria.component';
import { EvaluationCriteriaDetailComponent } from './page/protected/configuration/evaluation-criteria/detail/evaluation-criteria-detail.component';
import { ReportTypeComponent } from './page/protected/configuration/report-type/report-type.component';
import { ReportTypeDetailComponent } from './page/protected/configuration/report-type/detail/report-type-detail.component';
import { EvaluationBoardDetailComponent } from './page/protected/evaluation-board/detail/evaluation-board-detail.component';
import { ApiAccessComponent } from './page/protected/configuration/api-access/api-access.component';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'callback', component: CallbackComponent },
    { path: 'home', component: HomeComponent },
    { path: 'evaluation-board', component: EvaluationBoardComponent },
    { path: 'evaluation-board/:type', component: EvaluationBoardComponent },
    { path: 'evaluation-board/:type/detail/:id', component: EvaluationBoardDetailComponent },
    { path: 'reports', component: ReportsComponent },
    { 
      path: 'configuration/evaluation-object', 
      component: EvaluationObjectComponent,
      children: [
        { path: 'configuration', component: ConfigurationComponent },
        { path: 'role', component: RoleComponent },
        { path: '', redirectTo: 'configuration', pathMatch: 'full' }
      ]
    },
    { path: 'configuration/evaluation-flow', component: EvaluationFlowComponent },
    { path: 'configuration/evaluation-flow/new', component: EvaluationFlowDetailComponent },
    { path: 'configuration/evaluation-flow/:code/edit', component: EvaluationFlowDetailComponent },
    { path: 'configuration/evaluation-criteria', component: EvaluationCriteriaComponent },
    { path: 'configuration/evaluation-criteria/new', component: EvaluationCriteriaDetailComponent },
    { path: 'configuration/evaluation-criteria/:id/edit', component: EvaluationCriteriaDetailComponent },
    { path: 'configuration/report-type', component: ReportTypeComponent },
    { path: 'configuration/report-type/new', component: ReportTypeDetailComponent },
    { path: 'configuration/report-type/:id/edit', component: ReportTypeDetailComponent },
    { path: 'configuration/api-access', component: ApiAccessComponent },
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: '**', redirectTo: 'login' }, // Wildcard route for a 404 page
];
