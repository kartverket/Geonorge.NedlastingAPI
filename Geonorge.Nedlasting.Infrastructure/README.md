# Geonorge.Nedlasting.Infrastructure

This project will contain all implementations of external dependencies required in the download service. 

This is an ongoing refactoring process so some of the database access is still located within the web project.

The architecture priniciples is inspired by [Clean Architecture](https://github.com/ardalis/CleanArchitecture).

> Most of your application's dependencies on external resources should be implemented in classes defined in the Infrastructure project. These classes should implement interfaces defined in Core. If you have a very large project with many dependencies, it may make sense to have multiple Infrastructure projects (e.g. Infrastructure.Data), but for most projects one Infrastructure project with folders works fine. The sample includes data access and domain event implementations, but you would also add things like email providers, file access, web api clients, etc. to this project so they're not adding coupling to your Core or UI projects.

