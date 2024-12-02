# SQL Account Rest API

This project provides REST APIs to connect to **SQL Account** via its SDK. It also includes an embedded Vue.js-based frontend for monitoring logs and viewing API documentation. SQL Account is an on-premise application, and this project addresses the lack of APIs for integration by offering a robust solution for connecting SQL Account to third-party systems.

---

## Table of Contents

- [SQL Account Rest API](#sql-account-rest-api)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
    - [Controllers Summary](#controllers-summary)
  - [Prerequisites](#prerequisites)
  - [Usage](#usage)
    - [Deployment](#deployment)
    - [Local Development](#local-development)
    - [Security Notes](#security-notes)
  - [Development \& Contribution](#development--contribution)
  - [Deployment Recommendations](#deployment-recommendations)
  - [Contributors](#contributors)
  - [License](#license)

---

## Features

- **REST APIs for Integration**: Provides endpoints to interact with SQL Account data using SQL queries.
- **Vue.js Frontend**: Embedded dashboard located in `wwwroot/dashboard` for monitoring logs and viewing API documentation.
- **Database Querying**: Enables database operations on Firebird databases using SQL queries, with basic SQL injection protection.
- **Extensible Framework**: Built with .NET 8, making it easy to extend and contribute.
- **Secure Deployment**: Designed for secure access with recommendations for trusted environments and tools.

### Controllers Summary

1. **StockItemController**:
   - Manages stock items and inventory details.
2. **BizObjectController**:
   - Handles business object-related operations.
3. **CustomerController**:
   - Provides customer-related CRUD operations.
4. **SalesInvoiceController**:
   - Manages sales invoices and related transactions.
5. **CustomerInvoiceController**:
   - Handles customer invoices and financial details.
6. **StockAdjustmentController**:
   - Facilitates stock adjustment operations.
7. **CustomerPaymentController**:
   - Processes customer payments and records.
8. **SalesOrderController**:
   - Manages sales orders and related operations.
9. **HistoryController**:
   - Tracks history logs of operations.
10. **StockItemTemplateController**:
   - Manages stock item templates for predefined configurations.
11. **AppController**:
   - Provides application-level configurations and information.

---

## Prerequisites

Before using this project, ensure the following prerequisites are installed:

1. **Node.js (v18+)**  
   - [Download Node.js](https://nodejs.org/)
   - Follow the installation wizard for your operating system.

2. **.NET 8 Hosting Bundle**  
   - [Download .NET 8 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0)  
   - Choose the "Hosting Bundle" option to run .NET applications on IIS.

3. **.NET 8 SDK (for development)**  
   - [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)  
   - Install the SDK if you plan to contribute to the project or modify its codebase.

4. **SQL Account**  
   - [Download SQL Account](https://www.sql.com.my/products/)  
   - SQL Account must be installed and running on the system.

5. **IIS (Internet Information Services)**  
   - To enable IIS on Windows:
     1. Open **Control Panel** > **Programs** > **Programs and Features**.
     2. Click **Turn Windows features on or off**.
     3. Check the box for **Internet Information Services** and click **OK**.

---

## Usage

### Deployment

To deploy the application as a Windows Service or on IIS, follow the CLI instructions available at [SQL Accounting CLI](https://github.com/beehexacorp/sql-accounting-cli).

### Local Development

To run the project locally for debugging and implementation:

1. **Start the Frontend**:
   - Navigate to `wwwroot/dashboard` and run:
     ```bash
     npm run dev
     ```
   - Configure the `.env` file (similar to `env.example` in `wwwroot/dashboard`) to set the startup port for the frontend and the backend target URL.

2. **Start the Backend**:
   - Navigate to the backend project directory and run:
     ```bash
     dotnet watch run --project src/SqlAccountRestAPI/
     ```
   - Configure the `.env` file (similar to `env.example` in `src/SqlAccountRestAPI/`) to allow the frontend's CORS access based on the frontend port set in step 1.

3. **Hot Reloading**:
   - Both the frontend and backend support hot-reloading. You can edit files in either codebase, and changes will apply automatically without restarting the application.

### Security Notes

1. **SQL Injection Risk**: While basic SQL injection checks are implemented, queries should only be shared with trusted individuals. The SQL Account SDK does not support parameterized queries.
2. **API Exposure**: Avoid exposing the REST API to the internet. Use secure methods like Cloudflare Tunnel or VPN to restrict access.

---

## Development & Contribution

We welcome contributions to improve and extend this project.

- **Fire a Ticket**: If you encounter any issues, feel free to fire a ticket on our [GitHub repository](https://github.com/beehexacorp/sql-accounting-cli).
- **Contact Support**: Reach out to our team at [support@hexasync.com](mailto:support@hexasync.com).
- **Fork and Contribute**: You are welcome to fork the project, develop your features, and create pull requests (PRs) for review.

---

## Deployment Recommendations

For secure deployment:

- Use a trusted environment with restricted access.
- Configure logging and monitor API usage via the embedded dashboard.
- Regularly update and patch your environment to mitigate security risks.

---

## Contributors

This project is maintained and contributed by the **HexaSync Team**.

---

## License

This project is open-source. Contributions are highly encouraged!
