import { Cliente } from './cliente';
import { Empresa } from './empresa';

export interface NotaFiscal {
  id: number;
  numero: string;
  dataEmissao: string;
  status: string;
  cliente: Cliente;
  empresa: Empresa;
  quantidadeItens: number;
  valorTotal: number;
}
