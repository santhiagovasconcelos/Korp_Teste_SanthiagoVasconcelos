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
//importando o RxJS para função dentro da consulta do saldo do estoque
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { SaldoEstoque } from '../models/saldo-estoque';

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
  quantidade = 0;

  constructor(
    private notaFiscalService: NotaFiscalService,
    private produtoService: ProdutoService,
  ) {}

  notaSelecionada = signal<NotaFiscalDetalhe | null>(null);

  saldoProduto: SaldoEstoque | null = null;
  saldoSuficiente = false;

  //Variável do RxJS
  private produtoSelecionado$ = new Subject<number>();

  ngOnInit(): void {
    this.carregarNotas();
    this.carregarClientes();
    this.carregarEmpresas();
    this.carregarProdutos();

    this.produtoSelecionado$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((produtoId) => this.produtoService.consultarSaldo(produtoId)),
      )
      .subscribe({
        next: (saldo) => {
          this.saldoProduto = saldo;
          this.validarQuantidade();
        },
        error: (erro) => {
          console.error('Erro ao consultar saldo:', erro);
          this.saldoProduto = null;
          this.saldoSuficiente = false;
        },
      });
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

  processarNota(): void {
    const nota = this.notaSelecionada();

    if (!nota || nota.status !== 'Aberta') {
      return;
    }

    if (nota.itens.length === 0) {
      alert('A nota precisa possuir pelo menos um item para ser processada.');
      return;
    }

    const confirmar = confirm(`Deseja realmente processar a nota ${nota.numero}?`);

    if (!confirmar) {
      return;
    }

    this.notaFiscalService.processar(nota.id).subscribe({
      next: () => {
        this.carregarNotas();
        this.verDetalhes(nota.id);

        alert('Nota processada com sucesso.');
      },
      error: (erro) => {
        console.error('Erro ao processar nota:', erro);

        const mensagem =
          typeof erro.error === 'string'
            ? erro.error
            : (erro.error?.message ?? 'Não foi possível processar a nota.');

        alert(mensagem);
      },
    });
  }

  cancelarNota(): void {
    const nota = this.notaSelecionada();

    if (!nota || nota.status === 'Cancelada') {
      return;
    }

    const confirmar = confirm(`Deseja realmente cancelar a nota ${nota.numero}?`);

    if (!confirmar) {
      return;
    }

    this.notaFiscalService.cancelar(nota.id).subscribe({
      next: () => {
        this.carregarNotas();
        this.verDetalhes(nota.id);

        alert('Nota cancelada com sucesso.');
      },
      error: (erro) => {
        console.error('Erro ao cancelar nota:', erro);

        const mensagem =
          typeof erro.error === 'string'
            ? erro.error
            : (erro.error?.message ?? 'Não foi possível cancelar a nota.');

        alert(mensagem);
      },
    });
  }
  //verificar produto alterado para poder usar o RxJS
  produtoAlterado(produtoId: number): void {
    this.produtoId = produtoId;
    this.saldoProduto = null;
    this.saldoSuficiente = false;

    if (produtoId) {
      this.produtoSelecionado$.next(produtoId);
    }
  }

  validarQuantidade(): void {
    if (!this.saldoProduto) {
      this.saldoSuficiente = false;
      return;
    }

    const nota = this.notaSelecionada();

    const itemExistente = nota?.itens.find((item) => item.produtoId === this.produtoId);
    //Se o produto já existir nos itens,
    // o saldo deve levar em consideração na contagem do saldo
    const quantidadeNaNota = itemExistente?.quantidade ?? 0;

    const saldoDisponivel = this.saldoProduto.saldo - quantidadeNaNota;

    this.saldoSuficiente = this.quantidade > 0 && this.quantidade <= saldoDisponivel;
  }

  quantidadeJaNaNota(): number {
    const nota = this.notaSelecionada();

    const item = nota?.itens.find((item) => item.produtoId === this.produtoId);

    return item?.quantidade ?? 0;
  }

  saldoDisponivelParaAdicionar(): number {
    if (!this.saldoProduto) {
      return 0;
    }

    return this.saldoProduto.saldo - this.quantidadeJaNaNota();
  }
}
