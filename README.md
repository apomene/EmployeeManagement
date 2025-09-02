# EmployeeManagement Solution

Current solution contains following Projects:

- EmployeeManagement.API: ASP NET CORE WEB API As per MVP specs
- EmployeeManagement.API.Tests: UNIT test project that contains unit tests for above API using NUNIT
- EmployeeManagement.Models:  C# class library that contains all the shared Models, Entitties, DTOs etc of the solution
- EmployeeManagement.MVC: ASP NET CORE MVC project (Razor Views), that consumes the API  (UI client) as per MVP specs


# Technologies used

- .NET 9
- SQLlite as the RDBMS for the Basic API Model (Employees, Skills, etc)
- MongoDB as a NoSQL Database for logging History data of employee changes. In Production ENviroment this data should get huge over time, that is why we used an NoSQL DB



# FURTHER IMPROVEMENTS THAT CAN BE MADE

- Add Logging to MVC client.

- implementing role-based authentication to ensure that users/schedulers have access only to the resources and actions appropriate for their roles. ALso Audit logs will record the scheduler that made the change.

- Use caching mechanisms for both employee data and audit logs. Introducing caching mechanisms for both employee data and audit logs would significantly improve response times and reduce database load, leading to a more efficient and scalable application.
