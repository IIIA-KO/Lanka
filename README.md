# Lanka

**Lanka** is the enhanced evolution of the [Trendlink](https://github.com/IIIA-KO/Trendlink) project—a platform designed to connect Instagram influencers and small-to-medium businesses with advertisers. Lanka provides a unified service that delivers exclusive Instagram analytics, smart matching, and streamlined collaboration between content creators and advertisers.

## Overview

Lanka bridges the gap between content creators and brands by offering:

- **Enhanced Analytics:**  
  Gain access to in-depth Instagram statistics that go beyond what’s publicly available, empowering users to make informed, data-driven decisions.

- **Smart Matching:**  
  Utilize advanced filtering and ranking algorithms to identify the ideal advertising partners, based on exclusive and detailed data.

- **Streamlined Collaboration:**  
  Simplify the process of connecting influencers with advertisers, ensuring seamless partnerships and efficient campaign management.

## Project Status

Lanka is actively under development. Key backend modules are in place, including:

- **User Authentication:**  
  - Integration with Keycloak to enable secure user registration (login functionality is forthcoming).

- **Campaigns Module:**  
  - Basic implementation that includes:
    - **Bloggers:** Automatically created when a user registers (a snapshot of user data is maintained in the Campaigns module).
    - **Offers & Pacts:** Handling offer creation and management.
    - **Campaigns:** Managing campaign lifecycle with statuses such as Pending, Confirmed, Rejected, Done, and Completed.

The front-end, which will be developed in Angular, is still in the planning stage.

## Documentation

All architectural decisions, design documents, and implementation guidelines are maintained in the [docs](/docs/) directory. Please refer to these documents as the project evolves.

## Getting Started

Since Lanka is in active development, the current focus is on establishing the backend architecture and core functionalities. Detailed setup instructions will be provided as the project matures. Stay tuned for further updates on getting started with development.

## Contributing

We welcome contributions to Lanka! Your input is essential to help us build a robust and scalable platform. Before contributing, please review our [CONTRIBUTING.md](CONTRIBUTING.md) file, which outlines:

- **Issue Reporting:**  
  How to open issues using our standardized templates.
- **Branch Naming & Commit Message Conventions:**  
  Guidelines on how to name branches (e.g., `feature/123-add-login-endpoint`) and write clear commit messages that reference issue numbers.
- **GitHub Workflow:**  
  Our project board organization, label usage, and milestone tracking.
- **Pull Request Process:**  
  Steps to create and review pull requests.
