# Gerar Custo Abate e Desossa em .NET

Migracao inicial do projeto Lazarus/Delphi localizado em:

`C:\Visual Studio 2026\Projetos\1 -FrigoSul\1 - GerarCustoAbateDesossa\GerarCustoAbateDesossa`

## O que ja foi portado

- Aplicacao desktop em WinForms para abrir no Visual Studio 2026.
- Pesquisa de custo de abate e desossa por periodo e unidade.
- Processamento diario com confirmacao para refazer dias que ja possuem dados.
- Exportacao do grid para CSV.
- Leitura de `CONFIG.INI` fora do codigo-fonte.

## Estrutura

- `src/GerarCustoAbateDesossa.Desktop`: interface WinForms.
- `src/GerarCustoAbateDesossa.Application`: contratos e modelos de uso.
- `src/GerarCustoAbateDesossa.Domain`: enums e catalogos de dominio.
- `src/GerarCustoAbateDesossa.Infrastructure`: leitura de INI e acesso ADO.NET ao Oracle.

## Configuracao

Edite o arquivo `src/GerarCustoAbateDesossa.Desktop/CONFIG.INI` com:

- `ConnectionString`: conexao Oracle.
- `TnsAdmin`: pasta que contem `tnsnames.ora` e `sqlnet.ora` quando usar alias como `FRIGOSUL`.
- `ProviderAssemblyPath`: caminho para `Oracle.ManagedDataAccess.dll`, se necessario.
- `LibraryLocation`: opcional, para incluir o client Oracle nativo no `PATH`.

Se preferir, tambem pode preencher `DataSource`, `UserId` e `Password` separadamente no `CONFIG.INI`.

## Observacao importante

O sistema original tinha credenciais Oracle fixas no formulario Delphi. Na versao .NET, isso foi removido do codigo e passado para o `CONFIG.INI`.
