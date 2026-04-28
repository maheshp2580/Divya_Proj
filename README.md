# SmartJobRunner

## Overview of Project
SmartJobRunner is a robust background job execution and scheduling system. It provides an intuitive platform to manage, execute, and monitor automated tasks (such as HTTP requests and Database operations) using Hangfire. A standout feature of this system is its integration with Google's Gemini AI, which automatically analyzes background job failures to provide instant root cause analysis and suggested fixes.

## Why this Project
Managing, monitoring, and debugging background jobs in distributed systems can be challenging. Developers often spend significant time digging through logs to understand why a scheduled task failed. SmartJobRunner aims to solve this by providing a unified interface for scheduling tasks while leveraging Generative AI (Gemini) to act as a virtual assistant, instantly diagnosing failures and reducing system downtime.

## Features
- **Job Management:** Create, execute, schedule, and delete HTTP and Database jobs.
- **Flexible Scheduling:** Run jobs manually, immediately, or on a recurring basis using Cron expressions.
- **AI Failure Analysis:** Automatically captures job exceptions and queries the Gemini AI to provide a clear explanation and suggested fix.
- **Hangfire Dashboard Integration:** Built-in monitoring UI for tracking queued, processing, and failed jobs.
- **Configurable Resilience:** Support for retries and exponential backoff.
- **Demo Mode:** Built-in ability to simulate job failures (`SimulateFailureForDemo`) to test the AI diagnostic flow.

## Tech Stack
- **Backend Framework:** .NET (C#) / ASP.NET Core Web API
- **Architecture Pattern:** Clean Architecture & CQRS Pattern
- **Background Processing:** Hangfire
- **Mediator Pattern:** MediatR
- **Database:** SQLite & Entity Framework Core (EF Core)
- **Logging:** Serilog (Structured Logging)
- **AI Integration:** Google Gemini API

## Architecture
The application is built using **Clean Architecture** to ensure separation of concerns and maintainability. The codebase is divided into four main layers:
1. **API (SmartJobRunner.API):** The presentation layer hosting the RESTful controllers, Swagger documentation, and Hangfire Dashboard.
2. **Application (SmartJobRunner.Application):** Contains the business logic, interfaces, and CQRS handlers (Commands & Queries) using MediatR.
3. **Domain (SmartJobRunner.Domain):** Holds the core entities (`JobDefinition`, `JobExecution`) and Enums (`JobType`, `JobStatus`).
4. **Infrastructure (SmartJobRunner.Infrastructure):** Contains external system integrations, including the EF Core `ApplicationDbContext`, and the `GeminiAnalysisService`.

## API Endpoints
- `POST /api/jobs` - Create a new job definition.
- `GET /api/jobs` - Retrieve a list of all jobs.
- `GET /api/jobs/{id}` - Retrieve details of a specific job.
- `POST /api/jobs/{id}/execute` - Trigger an immediate execution of a job.
- `POST /api/jobs/{id}/schedule` - Schedule a job using a cron expression.
- `GET /api/jobs/{id}/executions` - Get the execution history for a job.
- `PUT /api/jobs/{id}/toggle-status` - Enable or disable a job.
- `DELETE /api/jobs/{id}` - Delete a job definition.

## Database
The project uses **SQLite** as its lightweight, relational database, managed through **Entity Framework Core**. 
- The database file (`smartjobrunner.db`) is automatically generated at startup based on the EF Core entities if it doesn't already exist.
- Connection string is managed in `appsettings.json`: `"Data Source=smartjobrunner.db"`.

## How to Run
1. **Prerequisites:** Ensure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed.
2. **Configure API Key:** Open `SmartJobRunner.API/appsettings.json` and set your Gemini API key under `"Gemini": { "ApiKey": "YOUR_KEY" }`, or set the `GEMINI_API_KEY` environment variable.
3. **Navigate to API Directory:**
   ```bash
   cd SmartJobRunner.API
   ```
4. **Run the Application:**
   ```bash
   dotnet run
   ```
5. The application will start, create the SQLite database automatically, and host the API locally.

## Swagger Link
Once the application is running, you can explore and interact with the API endpoints via Swagger UI at:
- **Swagger UI:** `https://localhost:<port>/swagger/index.html` or `http://localhost:<port>/swagger/index.html`

*Note: The exact port number can be found in the terminal output when you run the application.*

## Demo Flow
1. **Launch the Application:** Open the Swagger UI in your browser.
2. **Create a Job:** Use the `POST /api/jobs` endpoint to create an HTTP job. Set `SimulateFailureForDemo: true`.
3. **Execute the Job:** Trigger the job manually using `POST /api/jobs/{id}/execute`.
4. **View Execution:** The job will intentionally fail (due to the demo flag). Hangfire will catch the failure, and the system will automatically send the error context to the Gemini Analysis Service.
5. **Check Diagnostics:** Use `GET /api/jobs/{id}/executions` to view the job's execution history. You will see the failure alongside a human-readable AI analysis explaining why it failed and how to fix it.
6. **Monitor via Hangfire:** Navigate to `http://localhost:<port>/hangfire` to view the graphical dashboard of all background tasks.

## Future Improvements
- **Expanded Job Types:** Add support for messaging queues (RabbitMQ/Kafka) and file processing jobs.
- **Advanced Authentication:** Implement JWT-based authentication and Role-Based Access Control (RBAC).
- **Real-Time Monitoring:** Integrate SignalR for real-time dashboard updates on job statuses.
- **Enterprise Database Support:** Easy migration to SQL Server or PostgreSQL for production-scale deployments.
- **Multi-AI Provider Support:** Allow switching between different AI models (e.g., OpenAI, Claude) for diagnostics.

## API Info
The SmartJobRunner API is a standard RESTful service returning JSON responses. It utilizes HTTP status codes effectively (e.g., `200 OK`, `404 Not Found`, `204 No Content`). The CQRS pattern ensures that read operations (Queries) and write operations (Commands) are strictly separated, allowing for clean, testable, and scalable endpoint handlers.
