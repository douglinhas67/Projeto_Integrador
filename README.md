# Sistema de Controle de Treinos

Projeto integrador do SENAC, desenvolvido por Thomas e Igor em C# com MySQL.

O repositório contém o programa de console (`ControleTreinos`) e a interface
Windows Forms (`ControleTreinos.WinForms`). As duas versões utilizam os mesmos
modelos, repositórios e banco de dados.

## Executar no Windows

Pré-requisitos: SDK .NET 10, MySQL em execução e as tabelas criadas pelo arquivo
`Banco.sql`. No terminal PowerShell, na raiz do repositório:

```powershell
$env:CONTROLE_TREINOS_CONEXAO = 'Server=127.0.0.1;Port=3306;Database=controle_treinos;User ID=root;Password=SUA_SENHA_DO_MYSQL;'
dotnet run --project .\ControleTreinos.WinForms\ControleTreinos.WinForms.csproj
```

Substitua a senha e, se necessário, o endereço, a porta e o usuário. A variável
vale para o terminal atual; mantenha as credenciais fora do código e dos commits.

Para executar a versão de console nesse mesmo terminal:

```powershell
dotnet run --project .\ControleTreinos\ControleTreinos.csproj
```

## Interface gráfica

- Navegação lateral entre visão geral, usuários, treinos e resumo, dentro da mesma janela.
- Visão geral com totais do MySQL e até dez atividades recentes.
- Tabelas e formulários em áreas separadas, com filtros junto ao histórico.
- Cadastro e edição identificados no formulário; exclusão exige confirmação.
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

