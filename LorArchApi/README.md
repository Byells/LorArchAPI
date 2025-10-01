# LorArch API 🏍️

---

API para o sistema de gerenciamento de frotas de motos, desenvolvida como parte do projeto da disciplina de _Advanced Business Development with .NET_.

---

## 👨‍💻 Integrantes

- **Gabriel Lima Silva - RM556773**
- **Cauã Marcelo - RM558024**
- **Marcos Ramalho - RM554611**

---

## 🏗️ Arquitetura e Decisões de Design

A arquitetura deste projeto foi escolhida para ser moderna, performática, escalável e de fácil manutenção, seguindo as melhores práticas do ecossistema .NET.

- **Minimal APIs (.NET 9):** Escolhemos Minimal APIs pela sua simplicidade e alto desempenho. Esta abordagem reduz o código repetitivo ("boilerplate") encontrado em arquiteturas mais antigas como MVC, tornando a API mais leve e rápida, ideal para microsserviços e para consumo por aplicações mobile.

- **Entity Framework Core (Code-First):** Utilizamos o EF Core como ORM para abstrair a comunicação com o banco de dados Oracle. A abordagem "Code-First" com migrations permite que o esquema do banco de dados evolua junto com o código da aplicação, garantindo consistência e facilitando o desenvolvimento e o deploy.

- **Endpoints Modulares:** A API foi organizada separando os endpoints por funcionalidade em classes estáticas (ex: `UnidadeEndpoints`, `MotoEndpoints`). Essa estrutura mantém o `Program.cs` limpo e organiza o código de forma lógica e escalável, similar ao padrão de "Controllers".

- **Autenticação JWT (JSON Web Tokens):** Para a segurança, implementamos um sistema de autenticação baseado em tokens JWT. É um padrão de mercado, stateless e seguro, perfeito para proteger APIs que serão consumidas por diferentes clientes, como um aplicativo mobile.

- **Testes de Integração (xUnit):** Adicionamos uma suíte de testes de integração para garantir a qualidade e a confiabilidade dos endpoints principais. Utilizando uma `WebApplicationFactory` com um banco de dados em memória, simulamos requisições HTTP reais para validar o fluxo completo da aplicação, desde o recebimento da requisição até a resposta, garantindo que futuras alterações não quebrem funcionalidades existentes.

---

## 🚀 Instalação e Execução

### Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download "null")

- Acesso a um servidor de banco de dados Oracle

- Um editor de código ou IDE (JetBrains Rider, Visual Studio, VS Code)


### 1. Clone o Repositório

```
git clone [https://github.com/Byells/LorArchAPI.git](https://github.com/Byells/LorArchAPI.git)
cd LorArchAPI
```

### 2. Configure a Conexão com o Banco

Crie um arquivo `appsettings.Development.json` na raiz do projeto `LorArchApi` com a sua connection string do Oracle e as configurações do JWT:

```
{
  "ConnectionStrings": {
    "OracleDb": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=ENDERECO_DO_SERVIDOR:1521/SERVICE_NAME"
  },
  "Jwt": {
    "Key": "SuaChaveSuperSecretaDePeloMenos16Caracteres",
    "Issuer": "https://localhost:7161",
    "Audience": "https://localhost:7161"
  }
}
```

### 3. Aplique as Migrations

Com o Entity Framework Core Tools instalado (`dotnet tool install --global dotnet-ef`), execute o comando abaixo na pasta do projeto `LorArchApi` para criar as tabelas no banco de dados:

```
dotnet ef database update
```

### 4. Execute a API

```
dotnet run --project LorArchApi
```

A API estará rodando e o Swagger UI poderá ser acessado em **`https://localhost:7161/swagger/index.html`**.

## ✅ Executando os Testes

O projeto inclui uma suíte de testes de integração para validar os endpoints principais. Para executá-los, navegue até a pasta raiz da solução e rode o seguinte comando:

```
dotnet test
```

## 📚 Exemplos de Uso dos Endpoints

> **Novidades**: A nova versão da API contém **Paginação** e **HATEOAS** em seus endpoints!

Todos os endpoints, exceto os de registro e login, são protegidos e exigem um token de autenticação.

### 1. Autenticação

**A. Registrar um novo usuário**

Envie uma requisição `POST` para `/api/auth/register`:

```
{
  "email": "usuario@exemplo.com",
  "password": "Senha@123"
}
```

**B. Fazer login e obter um token**

Envie uma requisição `POST` para `/api/auth/login`:

```

{
  "email": "usuario@exemplo.com",
  "password": "Senha@123"
}
```

A resposta conterá o token JWT, que deverá ser usado nas próximas requisições.

### 2. Acessando um Endpoint Protegido

Para acessar qualquer outro endpoint, coloque na parte de Authorization do Swagger o que retornou do método de login`.

**Exemplo: Listar todas as unidades**

```
"Authorization: Bearer <TOKEN_JWT>"
```

## Observação

> **⚠️ Importante**: nos requests de **POST** e **PUT**, **não inclua** as propriedades de chave primária (os campos `Id...`) no corpo JSON. O servidor irá gerar/identificar automaticamente o ID. 😊

### Motos

* `GET    /motos`                     → Lista todas as motos
* `GET    /motos/{id}`                → Retorna moto por ID
* `POST   /motos`                     → Cria nova moto
* `PUT    /motos/{id}`                → Atualiza moto existente
* `DELETE /motos/{id}`                → Remove moto

### Unidades

* `GET    /unidades?cidade=&nome=`    → Lista unidades filtradas
* `GET    /unidades/{id}`             → Retorna unidade por ID
* `POST   /unidades`                  → Cria unidade
* `PUT    /unidades/{id}`             → Atualiza unidade
* `DELETE /unidades/{id}`             → Remove unidade

### Setores

* `GET    /setores?unidadeId=`        → Lista setores filtrados
* `GET    /setores/{id}`              → Retorna setor por ID
* `POST   /setores`                   → Cria setor
* `PUT    /setores/{id}`              → Atualiza setor
* `DELETE /setores/{id}`              → Remove setor

### Cidades

* `GET    /cidades?estadoId=&nome=`   → Lista cidades filtradas
* `GET    /cidades/{id}`              → Retorna cidade por ID
* `POST   /cidades`                   → Cria cidade
* `PUT    /cidades/{id}`              → Atualiza cidade
* `DELETE /cidades/{id}`              → Remove cidade

### Estados

* `GET    /estados?sigla=`            → Lista estados filtrados
* `GET    /estados/{id}`              → Retorna estado por ID
* `POST   /estados`                   → Cria estado
* `PUT    /estados/{id}`              → Atualiza estado
* `DELETE /estados/{id}`              → Remove estado

### Defeitos

* `GET    /defeitos?nome=`    → Lista defeitos filtrados por nome
* `GET    /defeitos/{id}`     → Retorna defeito por ID
* `POST   /defeitos`          → Cria novo defeito
* `PUT    /defeitos/{id}`     → Atualiza defeito existente
* `DELETE /defeitos/{id}`     → Remove defeito

### DefeitoMoto

* `GET    /defeitos-moto`        → Lista todos os registros de defeito de moto
* `GET    /defeitos-moto/{id}`   → Retorna registro de defeito de moto por ID
* `POST   /defeitos-moto`        → Cria novo registro de defeito de moto
* `PUT    /defeitos-moto/{id}`   → Atualiza registro existente
* `DELETE /defeitos-moto/{id}`   → Remove registro de defeito de moto

### Manutenções

* `GET    /manutencoes`        → Lista todas as manutenções
* `GET    /manutencoes/{id}`   → Retorna manutenção por ID
* `POST   /manutencoes`        → Cria nova manutenção
* `PUT    /manutencoes/{id}`   → Atualiza manutenção existente
* `DELETE /manutencoes/{id}`   → Remove manutenção

### Histórico de Movimentação

* `GET    /historicos`         → Lista todas as movimentações
* `GET    /historicos/{id}`    → Retorna movimentação por ID
* `POST   /historicos`         → Registra nova movimentação
* `PUT    /historicos/{id}`    → Atualiza movimentação existente
* `DELETE /historicos/{id}`    → Remove movimentação

### Localização

* `GET    /localizacoes`       → Lista todas as localizações
* `GET    /localizacoes/{id}`  → Retorna localização por ID
* `POST   /localizacoes`       → Cria nova localização
* `PUT    /localizacoes/{id}`  → Atualiza localização existente
* `DELETE /localizacoes/{id}`  → Remove localização

### LoRa

* `GET    /lora`              → Lista todos os dispositivos LoRa
* `GET    /lora/{id}`         → Retorna LoRa por ID
* `POST   /lora`              → Registra novo dispositivo LoRa
* `PUT    /lora/{id}`         → Atualiza dispositivo LoRa existente
* `DELETE /lora/{id}`         → Remove dispositivo LoRa

### RFID

* `GET    /rfid`              → Lista todos os tags RFID
* `GET    /rfid/{id}`         → Retorna RFID por ID
* `POST   /rfid`              → Registra novo tag RFID
* `PUT    /rfid/{id}`         → Atualiza tag RFID existente
* `DELETE /rfid/{id}`         → Remove tag RFID

---