# Sistema de Controle de Treinos

Projeto integrador do SENAC, desenvolvido por Thomas e Igor em C# com MySQL.

O repositório contém o programa de console (`ControleTreinos`) e a interface
Windows Forms (`ControleTreinos.WinForms`). As duas versões utilizam os mesmos
modelos, repositórios e banco de dados.

## Executar no Windows

Pré-requisitos: SDK .NET 10, MySQL em execução e as tabelas criadas pelo arquivo
`Banco.sql`. No terminal PowerShell, na raiz do repositório:

```powershell
$env:CONTROLE_TREINOS_CONEXAO = 'Server=127.0.0.1;Port=3306;Database=controle_treinos;User ID=root;Password=SENAC2026;'
dotnet run --project .\ControleTreinos.WinForms\ControleTreinos.WinForms.csproj
```

Substitua a senha e, se necessário, o endereço, a porta e o usuário. A variável
vale para o terminal atual; mantenha as credenciais fora do código e dos commits.

Para executar a versão de console nesse mesmo terminal:

```powershell
dotnet run --project .\ControleTreinos\ControleTreinos.csproj
```

No console, escolha **3 — Excluir usuário**, informe o identificador e confirme
com **S** ou cancele com **N**. A opção **2** permite consultar os identificadores.
A exclusão é bloqueada enquanto o usuário possuir treinos cadastrados.

## Interface gráfica

- Navegação lateral entre visão geral, usuários, treinos e resumo, dentro da mesma janela.
- Visão geral com totais do MySQL e até dez atividades recentes.
- Tabelas e formulários em áreas separadas, com filtros junto ao histórico.
- Cadastro e edição identificados no formulário; exclusão exige confirmação.
- Na tela Usuários, selecione uma pessoa e clique em **Excluir selecionado**.
  A exclusão é bloqueada enquanto ela possuir treinos; exclua esses treinos
  pela tela Treinos antes de remover o usuário.
- A troca de tela respeita o bloqueio durante operações e a confirmação para descartar uma edição de treino.

`Tema.cs` define as cores e fontes. `Componentes.cs` reúne os cartões,
indicadores e layouts compartilhados. A lógica de acesso ao MySQL continua
na pasta `ControleTreinos/Data`.

## Verificação no Windows

```powershell
dotnet build .\ControleTreinos.WinForms\ControleTreinos.WinForms.csproj -c Release
```

Ao abrir, confira a conexão e navegue pelas quatro áreas. Cadastre um usuário e
um treino, filtre o histórico por usuário, edite e cancele uma edição. Durante
uma edição, tente trocar de tela e verifique a confirmação. Teste cancelar e
confirmar uma exclusão, confira os totais no resumo e reabra o programa para
verificar a persistência. Confira também a janela maximizada, reduzida e na
escala de tela utilizada no computador da apresentação.

Na tela Usuários, confira que o botão de exclusão fica desabilitado sem seleção.
Selecione um usuário sem treinos, cancele a confirmação e confira que ele continua
na lista. Depois confirme a exclusão. Tente excluir um usuário com treinos e
confira que a operação é bloqueada, mantendo a pessoa e suas atividades.

## Teste de integração da exclusão de usuários

Use um banco MySQL **separado**, vazio ou reservado para testes, com nome
terminado em `_testes`. O usuário da conexão precisa poder criar as tabelas.
Configure `CONTROLE_TREINOS_TESTE_CONEXAO` com a conexão desse banco e execute:

```powershell
dotnet run --project .\tests\ControleTreinos.Integracao\ControleTreinos.Integracao.csproj
```

O teste verifica exclusão, usuário inexistente, persistência e preservação dos
treinos. Também executa o menu do console para testar identificadores inválidos,
confirmação, cancelamento e exclusão de usuários com e sem treinos. Ele remove
os registros que criou e não utiliza a conexão normal do aplicativo. A confirmação
e o cancelamento na janela gráfica devem ser conferidos no Windows.

## Validação de idade e duração na interface

Os dois campos validam o texto digitado antes de salvar. Valores como `1,5`,
`1.5`, zero, negativos, texto vazio e números maiores que `2147483647` são
rejeitados com uma mensagem, sem arredondamento ou conversão automática.

Para executar os testes do validador, sem precisar de MySQL ou Windows:

```powershell
dotnet run --project .\tests\ControleTreinos.Validacoes\ControleTreinos.Validacoes.csproj
```

No Windows, confira também os campos Idade e Duração ao cadastrar, editar e
corrigir uma entrada inválida. Uma duração inteira válida deve ser mantida ao
abrir e salvar a edição de um treino.

