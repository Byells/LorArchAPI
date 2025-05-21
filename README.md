# LorArch API

## Descrição do Projeto

A **LorArch API** é uma aplicação minimalista em **ASP.NET Core (.NET 9.0)** para gerenciamento completo de frotas de motos em pátios. Ela oferece:

* CRUD de **Motos**, **Unidades**, **Setores**, **Cidades**, **Estados**, **Defeitos**, **DefeitoMoto**, **Manutenções**, **Histórico de Movimentação**, **Localização**, **LoRa** e **RFID**.
* Integração com banco **Oracle** via **Entity Framework Core** e migrations.
* Documentação interativa com **Swagger / OpenAPI**.
* Filtros em endpoints via **QueryParams** e recursos em **PathParams**.

## Tecnologias

* **.NET 9.0 Minimal API**
* **Entity Framework Core** (Oracle)
* **Swashbuckle** (Swagger UI)
* **C# 13**

## Instalação e Configuração

1. **Pré‑requisitos**:

   * .NET 9.0 SDK
   * Acesso ao servidor Oracle
   * [JetBrains Rider](https://www.jetbrains.com/rider/)

2. **Clone o repositório**:

   ```bash
   https://github.com/Byells/LorArchAPI.git
   ```

3. **Configure a connection string**:

   * Crie `appsettings.Development.json` na raiz com:

     ```json
     {
       "ConnectionStrings": {
         "OracleDb": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/orcl"
       }
     }
     ```

4. **Aplicar Migrations**:

   * Pelo Rider: **Tools > Entity Framework > Migrations > Update Database**
   * Ou no terminal:

     ```bash
     dotnet ef database update
     ```

## Executando a API

```bash
dotnet run
```

Acesse o Swagger UI em: **[https://localhost:7161/swagger](https://localhost:7161/swagger/index.html)**

## Rotas e Endpoints Principais

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

*Desenvolvido por Gabriel Lima Silva para Advanced Business Development with .NET*
