# PearlMetric

PearlMetric is an enterprise-grade distributed analytics platform designed to calculate, log, and analyze time-series color progression metrics. The system leverages computer vision pipelines to process image inputs, track alignment matrices, and serve microservice analytics through a high-performance .NET gateway layer.

---

## System Architecture

The application is built as a decoupled, multi-service topology designed for high throughput, precise data processing, and clear separation of concerns.

```mermaid
graph TD
    subgraph PearlMetric [PearlMetric Analytical Data Pipeline]
        UI[Angular 22 Frontend]
        Node[Node 24 Gateway]
        NET[.NET 10 GatewayApi]
        Py[Python CV Worker]
        DB[(PostgreSQL 18 DB)]
    end

    UI -->|HTTP Requests| Node
    Node -->|Reverse Proxy| NET
    NET -->|Internal HTTP Call| Py
    NET -->|Entity Framework Core| DB
    
    style UI fill:#dd0031,stroke:#fff,stroke-width:2px,color:#fff
    style Node fill:#339933,stroke:#fff,stroke-width:2px,color:#fff
    style NET fill:#512bd4,stroke:#fff,stroke-width:2px,color:#fff
    style Py fill:#3776ab,stroke:#fff,stroke-width:2px,color:#fff
    style DB fill:#336791,stroke:#fff,stroke-width:2px,color:#fff
```

### Core Components
- Frontend UI: Built with Angular 22, providing a clean, responsive analytics dashboard for data visualization and progress tracking.
- Reverse Proxy / Gateway: Managed via Node 24 to handle request routing, transport security, and client communication.
- Core API Engine: Powered by .NET 10 Minimal APIs, managing business logic, orchestrating internal microservices, and handling structural database storage.
- Computer Vision Engine: An isolated Python worker task queue running color calibration loops and processing matrix analysis algorithms.
- Data Tier: A PostgreSQL 18 database utilizing Entity Framework Core for complex relational maps and time-series logging.

## Repository Structure
```
pearl-metric
├── .gitignore             # Global version control exclusion rules
├── docker-compose.yml     # Production-mirrored multi-container local orchestration
├── PearlMetric.slnx       # Modern .NET 10 lightweight solution format
├── README.md              # System documentation
└── src                    # Domain isolation root
    └── GatewayApi         # Core backend microservice
        ├── Data           # DBContext mappings and active persistence handlers
        ├── Models         # Strongly-typed data contract tables
        └── Program.cs     # Top-level application configuration entry point
```

## Getting Started
### Prerequisites
- Docker & Docker Compose
- .NET 10 SDK (pinned in `global.json`)
- Node.js 24 (pinned in `.nvmrc`; used later for Angular)

### Spin Up Infrastructure
Create an untracked local environment file and replace its placeholder password:

```
cp .env.example .env
```

To provision PostgreSQL locally, spin up the Docker network:

```
docker compose up -d
```

### Build and Verify
From the repository root:

```
./scripts/build.sh
./scripts/test.sh
```

### Run the API Engine
Navigate to the source directory and store a matching connection string in .NET user secrets. The password must match `POSTGRES_PASSWORD` in `.env`:

```
cd src/GatewayApi
dotnet user-secrets set ConnectionStrings:PearlMetric 'Host=localhost;Port=5432;Database=pearlmetric_dev;Username=pearladmin;Password=replace-with-your-local-password'
dotnet watch run
```

Alternatively, provide the connection string through the `ConnectionStrings__PearlMetric` environment variable. If the PostgreSQL volume was initialized previously, use that volume's existing password or recreate the local development volume.
