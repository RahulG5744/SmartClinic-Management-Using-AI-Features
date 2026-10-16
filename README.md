# SmartClinicSystem 🏥

A full-stack Hospital Management System built with **ASP.NET Core MVC (.NET 8)**, **Entity Framework Core**, and **SQL Server**, featuring patient/doctor/appointment management, an admin dashboard, and a built-in **AI Symptom Assistant** chat widget.

---

## 1. Features

- **Patient Management** — register, view, edit, delete, search patients + medical history
- **Doctor Management** — manage doctor profiles, specialization, availability
- **Appointment Booking** — book, edit, filter by status (Pending/Confirmed/Completed/Cancelled), cascading dropdowns
- **Admin Dashboard** — live counts of patients/doctors/appointments + recent activity, protected by a simple login
- **AI Symptom Assistant** — floating chat widget (bottom-right) that gives basic, rule-based guidance on common symptoms and always nudges users toward booking a real appointment
- Fully responsive Bootstrap 5 UI
- Auto-applies EF Core migrations on startup (no manual `update-database` needed after first deploy)

---

## 2. Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core MVC (.NET 8), C# |
| Data Access | Entity Framework Core (Code First) |
| Database | SQL Server (LocalDB for dev, Azure SQL / SQL Server for prod) |
| Frontend | Razor Views, Bootstrap 5, Font Awesome, vanilla JS |
| AI | Custom rule-based symptom-matching service (upgradeable to a real LLM — see §8) |

---

## 3. Prerequisites

Install these before you start:

1. **.NET 8 SDK** → https://dotnet.microsoft.com/download/dotnet/8.0
2. **SQL Server** — either:
   - **SQL Server LocalDB** (comes with Visual Studio's "ASP.NET and web development" workload), or
   - **SQL Server Express/Developer** edition, or
   - **Docker**: `docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`
3. **Visual Studio 2022** (Community edition is free) *or* VS Code + C# Dev Kit extension
4. **EF Core CLI tools**:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

Verify installs:
```bash
dotnet --version      # should show 8.x
dotnet ef --version   # should show a version number
```

---

## 4. Project Setup (from the files I've given you)

1. Download/copy the whole `SmartClinicSystem` folder to your machine (e.g. `C:\Projects\SmartClinicSystem` or `~/Projects/SmartClinicSystem`).
2. Open a terminal inside that folder.
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Confirm your **connection string** in `appsettings.json` matches your SQL Server setup:
   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartClinicDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   ```
   - Using LocalDB → leave as-is.
   - Using SQL Server Express → change to `Server=.\\SQLEXPRESS;...`
   - Using Docker/SQL auth → `Server=localhost,1433;Database=SmartClinicDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True`

---

## 5. Create the Database (EF Core Migrations)

Run these from the project root (where `.csproj` lives):

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates the `SmartClinicDb` database with `Patients`, `Doctors`, and `Appointments` tables, and seeds 2 sample doctors.

> Note: `Program.cs` also calls `db.Database.Migrate()` automatically at startup, so if you skip this step the app will still create the database the first time you run it — but running migrations manually first lets you catch any errors early.

---

## 6. Run the Project

```bash
dotnet run
```

Or press **F5** in Visual Studio.

Then open the URL shown in the terminal (e.g. `https://localhost:7230`).

**Admin login** (for the Dashboard): `admin` / `Admin@123` (defined in `appsettings.json` under `AdminCredentials` — change this before showing it to anyone else).

### Quick test checklist
- [ ] Home page loads with the hero section
- [ ] Register a new patient under **Patients → Register Patient**
- [ ] Book an appointment under **Appointments → Book Appointment**
- [ ] Log into **Admin Dashboard** and confirm the counts update
- [ ] Click the floating robot icon (bottom-right) and type "I have a fever" — confirm it replies

---

## 7. Project Structure

```
SmartClinicSystem/
├── Controllers/
│   ├── HomeController.cs        # landing page + admin dashboard
│   ├── PatientsController.cs    # patient CRUD
│   ├── DoctorsController.cs     # doctor CRUD
│   ├── AppointmentsController.cs# appointment CRUD + status filter
│   ├── AccountController.cs     # simple admin login/logout
│   └── ChatBotController.cs     # AI assistant API endpoint
├── Models/
│   ├── Patient.cs
│   ├── Doctor.cs
│   └── Appointment.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Services/
│   └── SymptomAssistantService.cs  # rule-based AI logic
├── Views/
│   ├── Home/, Patients/, Doctors/, Appointments/, Account/, Shared/
├── wwwroot/
│   ├── css/site.css
│   └── js/chatbot.js
├── Program.cs
├── appsettings.json
└── SmartClinicSystem.csproj
```

---

## 8. Upgrading the AI Assistant (Optional, for extra resume points)

The current assistant (`Services/SymptomAssistantService.cs`) is keyword/rule-based — this was a deliberate choice so the project runs anywhere with **zero external dependencies or API keys**, which matters for a portfolio project you want to demo reliably.

If you want to say "integrated a real LLM" on your resume, you can swap it for a real API call:

1. Get an API key (e.g. from Anthropic or OpenAI).
2. Store it safely using `dotnet user-secrets` (never commit it to GitHub):
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "AI:ApiKey" "your-key-here"
   ```
3. Replace the body of `SymptomAssistantService.GetResponse()` with an `HttpClient` call to the LLM API, passing the user's message and returning the model's reply.
4. Keep the current rule-based logic as a **fallback** if the API call fails — this shows good engineering judgment in an interview.

Mention this upgrade path in interviews even if you don't implement it — it shows you understand the tradeoff between reliability and capability.

---

## 9. Deployment — Step by Step

### Option A: Deploy free on Render.com (easiest, good for a resume link)

1. Push your project to a GitHub repository (see §10 below).
2. Create a free SQL Server-compatible database — Render doesn't offer free SQL Server, so for a free deploy, easiest is to **switch to a free-tier Azure SQL Database** (see Option B) and point Render's web service at it. Alternatively, deploy on Azure entirely (Option B) since it natively supports SQL Server end-to-end.

### Option B: Deploy on Azure (recommended — native SQL Server support, free tier available)

1. Create a free Azure account (https://azure.microsoft.com/free/) if you don't have one.
2. In the Azure Portal:
   - Create an **Azure SQL Database** (choose the free/serverless tier for a student project). Note the connection string.
   - Create an **Azure App Service** (choose the Free F1 or Basic B1 plan, .NET 8 stack).
3. In your local project, update `appsettings.json`'s `DefaultConnection` to the Azure SQL connection string (or better — set it as an **App Service Application Setting** named `ConnectionStrings__DefaultConnection` so it's not hardcoded).
4. Publish from Visual Studio: right-click the project → **Publish** → **Azure** → **Azure App Service** → follow the wizard. Or via CLI:
   ```bash
   dotnet publish -c Release -o ./publish
   az webapp deployment source config-zip --resource-group <your-rg> --name <your-app-name> --src ./publish.zip
   ```
5. Visit `https://<your-app-name>.azurewebsites.net` — the app will run its EF Core migration automatically on first request.

### Option C: Deploy on your own Windows Server / IIS

1. Install the **.NET 8 Hosting Bundle** on the server.
2. Publish locally: `dotnet publish -c Release -o ./publish`
3. Copy the `publish` folder to the server, create an IIS site pointing to it, set the Application Pool to "No Managed Code."
4. Ensure SQL Server is reachable from the server and the connection string is updated.

---

## 10. Push to GitHub (for your resume link)

```bash
cd SmartClinicSystem
git init
git add .
git commit -m "Initial commit: SmartClinicSystem - full stack hospital management app"
git branch -M main
git remote add origin https://github.com/<your-username>/SmartClinicSystem.git
git push -u origin main
```

Your `.gitignore` already excludes `bin/`, `obj/`, and other build artifacts.

**Important:** Before pushing publicly, change the default admin password in `appsettings.json`, or better, move it into `dotnet user-secrets` / environment variables so it isn't visible in your public repo.

---

## 11. Resume Bullet Points (ready to use)

- Built **SmartClinicSystem**, a full-stack Hospital Management System using ASP.NET Core MVC (.NET 8), Entity Framework Core, and SQL Server, implementing complete CRUD workflows for patients, doctors, and appointments.
- Designed a normalized relational database schema with EF Core Code-First migrations, including cascading relationships between patients, doctors, and appointments.
- Implemented an **AI-powered Symptom Assistant** chat widget using a custom rule-based NLP matching service exposed via a REST API endpoint, improving patient self-service and engagement.
- Built a real-time **Admin Dashboard** with live statistics (patient/doctor/appointment counts, pending appointments) using LINQ aggregation queries.
- Developed a responsive, accessible Bootstrap 5 UI with search, filtering, and status-based workflows for appointment management.
- Deployed the application to [Azure App Service / IIS], configuring environment-based settings and automated database migrations for zero-downtime releases.

---

## 12. Troubleshooting

| Problem | Fix |
|---|---|
| `A network-related or instance-specific error...` on `dotnet ef database update` | SQL Server isn't running, or the connection string is wrong. Check SQL Server service status / Docker container. |
| `Login failed for user` | Check `User Id`/`Password` in the connection string, or use `Trusted_Connection=True` for Windows auth. |
| Migrations folder already has files but schema is out of sync | Delete the `Migrations` folder and the database, then re-run `dotnet ef migrations add InitialCreate` and `dotnet ef database update`. |
| Chat widget doesn't respond | Open browser dev tools → Network tab → check the `/api/ChatBot/ask` call for errors. Make sure the app is running (not just the static files). |
| Port already in use | Change the port in `Properties/launchSettings.json`. |

---

Good luck — this project alone (CRUD + relational DB design + a working AI feature + deployment) is genuinely enough to stand out for a fresher-level .NET developer role. Focus your interview prep on explaining *why* you structured the DbContext relationships and the AI service the way you did — that's what interviewers actually probe.
