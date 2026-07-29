# 🏥 Hospital Shift & Employee Management System

An enterprise-level, end-to-end solution designed specifically for hospital administration to manage medical staff, automate complex shift scheduling (Morning and Night calls), and generate highly optimized, print-ready official reports. 

Developed with a focus on high performance, clean architecture, and intuitive user experience for healthcare environments.

---

## 📸 System Screenshots



<div align="center">

### 🖥️ 1. Main Dashboard & Shift Settings
![Settings Interface](https://github.com/user-attachments/assets/ecc29f10-563f-4a92-bc05-4d28eeaa4baf)
*Centralized dashboard to configure reference dates, search employees, and assign night shift supervisors dynamically.*

### 📄 2. Print-Ready Shift Schedule (Landscape)
![Report Preview](https://github.com/user-attachments/assets/20d66aa4-98cd-4327-a97b-14375afbad22)
*Auto-generated, balanced A4 Landscape report displaying morning shifts and the 2x2 grid for night call teams.*

</div>

---

## ✨ Key Features

### 📅 Advanced Shift Scheduling Engine
* **Morning Shifts:** Automatically categorizes and balances staff into "Saturday" and "Thursday" groups.
* **Night Shifts (Call Duties):** Implements a highly accurate **Modulo-4 Algorithm** that calculates the exact rotation of 4 night teams based on a customizable Reference Date.
* **Supervisor Assignment:** Smart search and auto-complete functionality to assign specific supervisors (e.g., Head Nurses/Doctors) to specific night teams.

### 🖨️ Dynamic & Smart Reporting (WPF FlowDocument)
* **Landscape Formatting:** Reports are generated natively in A4 Landscape mode to maximize horizontal space.
* **Auto-Balancing Tables:** The system mathematically calculates the maximum number of employees across teams and injects balanced spacing, ensuring all UI boxes remain perfectly aligned and symmetrical.
* **Space Optimization (Internal Columns):** Night shift tables automatically divide long employee lists into internal 2-column grids, preventing the schedule from overflowing onto a second page.
* **Official Hospital Formatting:** Prints without UI clutter, featuring official department titles, clean borders, and optimized typography for ink-saving printing.

### 🗄️ Comprehensive HR Management
* Track employee details (Birthdate, Hire Date, Gender, Phone, Address).
* Manage specific medical job titles and certificates.
* Track Job Status (Continuous, On Vacation, Departed, Retired).
* Leave and Absence tracking integrated with the employee profile.

---

## 🛠️ Technology Stack

### **Back-End (RESTful API)**
* **Framework:** .NET 8 Web API
* **ORM:** Entity Framework Core (EF Core)
* **Database:** Microsoft SQL Server
* **Architecture:** Clean Architecture, Repository Pattern (IShiftService, etc.)
* **Features:** LINQ queries, DTO mapping, asynchronous operations.

### **Front-End (Desktop Client)**
* **Framework:** WPF (.NET 8)
* **Design Pattern:** MVVM (Model-View-ViewModel)
* **UI Components:** Native WPF Controls, FlowDocumentPageViewer, PrintDialog.
* **Data Binding:** INotifyPropertyChanged, ObservableCollections, RelayCommands.

---
## 🚀 Getting Started
### **Prerequisites**
* **.NET 8.0 SDK or later**

* **Microsoft SQL Server (LocalDB or dedicated instance)**

* **Visual Studio 2022 (Recommended) or VS Code**
---
## Installation & Setup
* **Clone the Repository:**

Bash
git clone [https://github.com/your-username/hospital-shift-management.git](https://github.com/your-username/hospital-shift-management.git)
cd hospital-shift-management
* **Database Configuration:**

* **Open Hospital.API/appsettings.json.**

* **Update the DefaultConnection string to point to your SQL Server instance.**

* **Apply Database Migrations:**
Open your terminal in the Hospital.API directory and run:

Bash
dotnet ef database update
Run the API Server:

Bash
dotnet run --project Hospital.API
The API will start at https://localhost:xxxx. Ensure this URL matches the base URL in your WPF ApiService.

Launch the Desktop Application:
Open a new terminal window and run:

Bash
dotnet run --project Hospital.Desktop


##👤 Author
Sajjad Ali Mohsin

Nurse & Software Developer

Specializing in Backend Systems, Database Architecture, and Healthcare Software Solutions.

##📄 License
This project is licensed under the MIT License. See the LICENSE file for more details.
---

## 📐 Project Architecture

```text
HospitalSystem/
├── Hospital.API/                 # Backend RESTful API
│   ├── Controllers/              # API Endpoints (Shifts, Employees, etc.)
│   ├── Data/                     # ApplicationDbContext
│   └── Services/                 # Business Logic & Shift Calculation Engine
│
├── Hospital.Core/                # Shared Class Library
│   ├── Models/                   # Database Entities (Employee, NightShiftTeam, etc.)
│   ├── DTOs/                     # Data Transfer Objects
│   └── Enums/                    # System Enums (ShiftType, MorningGroups, JobStatus)
│
└── Hospital.Desktop/             # WPF Client Application
    ├── Services/                 # API Connection & ReportGenerator (FlowDocument)
    ├── ViewModels/               # MVVM Logic (ShiftSettingsViewModel)
    └── Views/                    # XAML Windows (ReportPreviewWindow, Main)
---
