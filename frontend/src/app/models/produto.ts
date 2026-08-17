export interface Produto {
  id?: number;
  codigo: string;
  descricao: string;
  preco: number;
  saldo: number;
  ativo: boolean;
}