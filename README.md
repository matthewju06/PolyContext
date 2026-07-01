# PolyContext

An automated intelligence pipeline and data platform designed to reduce informational asymmetry in prediction markets (such as Polymarket and Kalshi). The system ingests highly volatile public contract events, coordinates target-oriented web scraping and sentiment gathering, and presents structured contextual summaries alongside historical token price trends.

## System Architecture

The platform uses a polyglot microservice layout to leverage the specific strengths of both ecosystem frameworks:

*   **API Gateway (C# / .NET 10 Minimal API):** Manages real-time platform contract ingestion, event-driven cron polling, and robust data delivery to the user interface.
*   **Database (PostgreSQL 16):** Acts as the central relational data store, managing core entities, historical order book snapshots, and synthesized insights via Entity Framework Core 10.
*   **Research & Synthesis Cluster (Python):** Operates as an asynchronous background worker tasking specialized scrapers, querying target search endpoints, and utilizing LLM interfaces to parse source text into structured timelines.
*   **User Interface (Angular):** A scannable, performance-oriented intelligence dashboard displaying active contracts with real-time catalysts and contract clause vulnerability notices.

## Technical Stack

*   **Backend:** .NET 10 Web API, Entity Framework Core 10
*   **Data Science / Scraping:** Python 3.11+, PostgreSQL 16
*   **Frontend:** Angular
*   **Infrastructure:** Docker, Docker Compose

## Repository Directory Structure

```text
PolyContext/
├── .github/                  # CI/CD Workflows and project configurations
├── src/
│   ├── GatewayApi/           # .NET 10 Minimal API Gateway service
│   ├── ResearchWorker/       # Python background scraping and LLM synthesis worker
│   └── ClientApp/            # Angular frontend dashboard application
├── infra/
│   └── database/             # Persistent SQL storage volume mounts
├── PolyContext.sln           # Root .NET enterprise solution file
└── docker-compose.yml        # Local multi-container environment manifest