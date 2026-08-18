import { Component, OnInit, signal } from '@angular/core';
import { Produto } from '../models/produto';
import { ProdutoService } from '../services/produto';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-produtos',
  imports: [FormsModule, CurrencyPipe],
  templateUrl: './produtos.html',
  styleUrl: './produtos.scss',
})
export class Produtos implements OnInit {
  produtos = signal<Produto[]>([]);
  produtoEmEdicao: Produto | null = null;

  novoProduto: Produto = {
    codigo: '',
    descricao: '',
    preco: 0,
    saldo: 0,
    ativo: true,
  };

  limparFormulario(): void {
    this.produtoEmEdicao = null;

    this.novoProduto = {
      codigo: '',
      descricao: '',
      preco: 0,
      saldo: 0,
      ativo: true,
    };
  }

  salvarProduto(): void {
    if (this.produtoEmEdicao) {
      this.produtoService.editar(this.novoProduto).subscribe({
        next: () => {
          this.carregarProdutos();
          this.limparFormulario();
        },
        error: (erro) => {
          console.error('Erro ao editar produto:', erro);
        },
      });
    } else {
      this.produtoService.cadastrar(this.novoProduto).subscribe({
        next: () => {
          this.carregarProdutos();
          this.limparFormulario();
        },
        error: (erro) => {
          console.error('Erro ao cadastrar produto:', erro);
        },
      });
    }
  }

  constructor(private produtoService: ProdutoService) {}
  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.produtoService.listar().subscribe({
      next: (dados) => {
        this.produtos.set(dados);
      },
      error: (erro) => {
        console.error('Erro ao carregar produtos:', erro);
      },
    });
  }

  editarProduto(produto: Produto): void {
    this.produtoEmEdicao = produto;

    this.novoProduto = {
      ...produto,
    };
  }

  // Garante que o produto tenha um ID antes da exclusão
  excluirProduto(produto: Produto): void {
    if (!produto.id) {
      return;
    }

    const confirmar = confirm(`Deseja realmente excluir o produto "${produto.descricao}"?`);

    if (!confirmar) {
      return;
    }

    this.produtoService.excluir(produto.id).subscribe({
      next: () => {
        this.carregarProdutos();
      },
      error: (erro) => {
        console.error('Erro ao excluir produto:', erro);
      },
    });
  }
}
