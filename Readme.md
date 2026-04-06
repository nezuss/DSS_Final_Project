# How to run (total 2 types of run)

> Forntend was made by Teacher (idk his github account name to add him in Contributors)  
> Forntend file was edited `docker-compose.yml`. Field Jwt__Key - super_secret_dev_key_change_me to ThisIsASecretKeyForJwtTokenGeneration

---

Firstly clone the repository `git clone https://github.com/nezuss/DSS_Final_Project.git`

## Localy
If you already have a database:
 - Go to backend directory and edit `appsettings.json` with your database and JWT credentials.

If not:
 - Run this command in CMD/PowerShell/Terminal(Linux/MacOS) - `docker compose -f docker-compose.local.yml up --build`
 - Use this param in `DefaultConnection` - `Server=127.0.0.1`
 - Go to backend directory and edit `appsettings.json` with your JWT credentials.

Run your backend using this command in the backend directory `dotnet run`

Go to the frontend directory and run this commands in 2 diffrent CMD/PowerShell/Terminal(Linux/MacOS):
 - (Window 0) Install npm packages `npm i`
 - (Window 0) Run frontend `npm run dev`
 - (Window 1) Run test `npm run test:e2e` or you can run tests manualy `npm run cy:open`
 
Done

---

## In docker
Go to the backend directory and run a build `docker compose build`

Setup your PowerShell or Teminal(Linux/MacOS)

### PowerShell
 - Setup Docker Image `$env:BACKEND_IMAGE="dss_final_project-backend:latest"`
 - Run this command to run the project in frontend directory `docker compose -f docker-compose.e2e.yml up --build --exit-code-from cypress`
 - Done

### Teminal(Linux/MacOS)
 - Setup Docker Image `export BACKEND_IMAGE="dss_final_project-backend:latest"`
 - Run this command to run the project in frontend directory `docker compose -f docker-compose.e2e.yml up --build --exit-code-from cypress`
 - Done
