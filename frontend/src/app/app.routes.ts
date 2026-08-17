import { Routes } from '@angular/router';
import { Produtos } from './produtos/produtos';
import { NotasFiscais } from './notas-fiscais/notas-fiscais';

export const routes: Routes = [
 {
    path: 'produtos',
    component: Produtos
  },
  {
    path: 'notas-fiscais',
    component: NotasFiscais
  }
];
