import { Cliente } from './cliente';
import { Empresa } from './empresa';
import { ItemNota } from './item-nota';

export interface NotaFiscalDetalhe {
  id: number;
  numero: string;
  dataEmissao: string;
  status: string;
  cliente: Cliente;
  empresa: Empresa;
  itens: ItemNota[];
}
