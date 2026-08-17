import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Cliente } from '../models/cliente';
import { Empresa } from '../models/empresa';
import { NotaFiscal } from '../models/nota-fiscal';
import { NotaFiscalDetalhe } from '../models/nota-fiscal-detalhe';
import { ItemNota } from '../models/item-nota';

interface CriarNotaRequest {
  clienteId: number;
  empresaId: number;
}

@Injectable({
  providedIn: 'root',
})
export class NotaFiscalService {
  private apiUrl = 'http://localhost:5150/api/Notas';

  constructor(private http: HttpClient) {}

  criar(dados: CriarNotaRequest): Observable<NotaFiscalDetalhe> {
    return this.http.post<NotaFiscalDetalhe>(this.apiUrl, dados);
  }

  listar(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.apiUrl);
  }

  //Carregar detalhes da nota
  buscarPorId(id: number): Observable<NotaFiscalDetalhe> {
    return this.http.get<NotaFiscalDetalhe>(`${this.apiUrl}/${id}`);
  }

  listarClientes(): Observable<Cliente[]> {
    return this.http.get<Cliente[]>('http://localhost:5150/api/clientes');
  }

  listarEmpresas(): Observable<Empresa[]> {
    return this.http.get<Empresa[]>('http://localhost:5150/api/empresas');
  }

  adicionarItem(notaId: number, produtoId: number, quantidade: number): Observable<ItemNota> {
    return this.http.post<ItemNota>(`${this.apiUrl}/${notaId}/itens`, {
      produtoId,
      quantidade,
    });
  }

  atualizarQuantidade(notaId: number, itemId: number, quantidade: number): Observable<ItemNota> {
    return this.http.put<ItemNota>(`${this.apiUrl}/${notaId}/itens/${itemId}`, {
      quantidade,
    });
  }

  removerItem(notaId: number, itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${notaId}/itens/${itemId}`);
  }

  processar(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/processar`, {});
  }

  cancelar(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancelar`, {});
  }
}
