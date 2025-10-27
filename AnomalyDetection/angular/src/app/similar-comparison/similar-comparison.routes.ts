import { Routes } from '@angular/router';
import { SimilarSignalSearchComponent } from './components/similar-signal-search/similar-signal-search.component';
import { TestDataListComponent } from './components/test-data-list/test-data-list.component';
import { ComparisonAnalysisComponent } from './components/comparison-analysis/comparison-analysis.component';
import { DataVisualizationComponent } from './components/data-visualization/data-visualization.component';

export const SIMILAR_COMPARISON_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'search',
    pathMatch: 'full'
  },
  {
    path: 'search',
    component: SimilarSignalSearchComponent,
    data: { title: '類似CAN信号検索 / Similar Signal Search' }
  },
  {
    path: 'test-data',
    component: TestDataListComponent,
    data: { title: '過去検査データ / Test Data History' }
  },
  {
    path: 'compare-analysis',
    component: ComparisonAnalysisComponent,
    data: { title: '比較分析 / Comparison Analysis' }
  },
  {
    path: 'visualization',
    component: DataVisualizationComponent,
    data: { title: 'データ可視化 / Data Visualization' }
  }
];
