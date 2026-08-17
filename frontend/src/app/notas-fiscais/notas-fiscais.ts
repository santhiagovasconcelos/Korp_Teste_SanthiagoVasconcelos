import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NotaFiscal } from '../models/nota-fiscal';
import { NotaFiscalService } from '../services/nota-fiscal';
import { NotaFiscalDetalhe } from '../models/nota-fiscal-detalhe';
import { Cliente } from '../models/cliente';
import { Empresa } from '../models/empresa';
import { Produto } from '../models/produto';
import { ProdutoService } from '../services/produto';
import { ItemNota } from '../models/item-nota';

@Component({
  selector: 'app-notas-fiscais',
  imports: [FormsModule],
  templateUrl: './notas-fiscais.html',
  styleUrl: './notas-fiscais.scss',
})
export class NotasFiscais implements OnInit {
  notas = signal<NotaFiscal[]>([]);

  clientes = signal<Cliente[]>([]);
  empresas = signal<Empresa[]>([]);

  clienteId = 0;
  empresaId = 0;

  produtos = signal<Produto[]>([]);

  produtoId = 0;
  quantidade = 1;

  constructor(
    private notaFiscalService: NotaFiscalService,
    private produtoService: ProdutoService,
  ) {}

  notaSelecionada = signal<NotaFiscalDetalhe | null>(null);

  ngOnInit(): void {
    this.carregarNotas();
    this.carregarClientes();
    this.carregarEmpresas();
    this.carregarProdutos();
  }

  carregarClientes(): void {
    this.notaFiscalService.listarClientes().subscribe({
      next: (dados) => {
        this.clientes.set(dados);
      },
      error: (erro) => {
        console.error('Erro ao carregar clientes:', erro);
      },
    });
  }

  carregarEmpresas(): void {
    this.notaFiscalService.listarEmpresas().subscribe({
      next: (dados) => {
        this.empresas.set(dados);
      },
      error: (erro) => {
        console.error('Erro ao carregar empresas:', erro);
      },
    });
  }

  carregarNotas(): void {
    this.notaFiscalService.listar().subscribe({
      next: (dados) => {
        this.notas.set(dados);
      },
      error: (erro) => {
        console.error('Erro ao carregar notas fiscais:', erro);
      },
    });
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

  criarNota(): void {
    // Garante que cliente e empresa foram selecionados antes de criar a nota
    if (!this.clienteId || !this.empresaId) {
      return;
    }

    this.notaFiscalService
      .criar({
        clienteId: this.clienteId,
        empresaId: this.empresaId,
      })
      .subscribe({
        next: () => {
          this.carregarNotas();

          this.clienteId = 0;
          this.empresaId = 0;
        },
        error: (erro) => {
          console.error('Erro ao criar nota fiscal:', erro);
        },
      });
  }

  verDetalhes(id: number): void {
    this.notaFiscalService.buscarPorId(id).subscribe({
      next: (dados) => {
        this.notaSelecionada.set(dados);
      },
      error: (erro) => {
        console.error('Erro ao carregar detalhes da nota:', erro);
      },
    });
  }

  adicionarItem(): void {
    const nota = this.notaSelecionada();

    if (!nota || nota.status !== 'Aberta') {
      return;
    }

    if (!this.produtoId || this.quantidade <= 0) {
      return;
    }

    this.notaFiscalService.adicionarItem(nota.id, this.produtoId, this.quantidade).subscribe({
      next: () => {
        this.carregarNotas();
        this.verDetalhes(nota.id);

        this.produtoId = 0;
        this.quantidade = 1;
      },
      error: (erro) => {
        console.error('Erro ao adicionar item:', erro);

        const itemExistente = nota.itens.find((item) => item.produtoId === this.produtoId);

        if (itemExistente) {
          const confirmar = confirm(
            `O produto "${itemExistente.descricaoProduto}" já está na nota.\n\n` +
              `Quantidade atual: ${itemExistente.quantidade}\n` +
              `Quantidade informada: ${this.quantidade}\n\n` +
              `Deseja somar as quantidades?`,
          );

          if (!confirmar) {
            return;
          }

          const novaQuantidade = itemExistente.quantidade + this.quantidade;

          this.notaFiscalService
            .atualizarQuantidade(nota.id, itemExistente.id, novaQuantidade)
            .subscribe({
              next: () => {
                this.carregarNotas();
                this.verDetalhes(nota.id);

                this.produtoId = 0;
                this.quantidade = 1;
              },
              error: (erroAtualizacao) => {
                console.error('Erro ao atualizar quantidade:', erroAtualizacao);

                const mensagem =
                  typeof erroAtualizacao.error === 'string'
                    ? erroAtualizacao.error
                    : (erroAtualizacao.error?.message ??
                      'Não foi possível atualizar a quantidade.');

                alert(mensagem);
              },
            });

          return;
        }

        const mensagem =
          typeof erro.error === 'string'
            ? erro.error
            : (erro.error?.message ?? 'Não foi possível adicionar o item.');

        alert(mensagem);
      },
    });
  }

  editarQuantidade(item: ItemNota): void {
    const nota = this.notaSelecionada();

    if (!nota || nota.status !== 'Aberta') {
      return;
    }

    const novaQuantidade = prompt(
      `Informe a nova quantidade para "${item.descricaoProduto}":`,
      item.quantidade.toString(),
    );
    //Caso não seja informado uma nova quantidade, clicando no cancelar
    if (novaQuantidade === null) {
      return;
    }

    //Correção devido retorno do compando prompt trazer como string.
    const quantidade = Number(novaQuantidade);

    if (!Number.isInteger(quantidade) || quantidade <= 0) {
      alert('Informe uma quantidade válida.');
      return;
    }

    this.notaFiscalService.atualizarQuantidade(nota.id, item.id, quantidade).subscribe({
      next: () => {
        this.carregarNotas();
        this.verDetalhes(nota.id);
      },
      error: (erro) => {
        console.error('Erro ao alterar quantidade:', erro);

        const mensagem =
          typeof erro.error === 'string'
            ? erro.error
            : (erro.error?.message ?? 'Não foi possível alterar a quantidade.');

        alert(mensagem);
      },
    });
  }

  removerItem(item: ItemNota): void {
    const nota = this.notaSelecionada();

    if (!nota || nota.status !== 'Aberta') {
      return;
    }

    const confirmar = confirm(
      `Deseja realmente remover o produto "${item.descricaoProduto}" da nota?`,
    );

    if (!confirmar) {
      return;
    }

    this.notaFiscalService.removerItem(nota.id, item.id).subscribe({
      next: () => {
        this.carregarNotas();
        this.verDetalhes(nota.id);
      },
      error: (erro) => {
        console.error('Erro ao remover item:', erro);

        const mensagem =
          typeof erro.error === 'string'
            ? erro.error
            : (erro.error?.message ?? 'Não foi possível remover o item.');

        alert(mensagem);
      },
    });
  }
}
