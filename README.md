# 🩸 BloodCenterOS  
### Open Source Blood Center Management System for India  
Developed & Maintained by **Sadu Consultancy Services**  
🌐 [www.saducs.com](http://www.saducs.com/bloodcenteros) | 📍 Pune, Maharashtra, India | 📞 +91 9765975757  

---

## 🇮🇳 Overview

**BloodCenterOS** is an open-source, cloud-ready **Blood Center Management System** designed for **Indian Blood Centers** (as per NBTC/NACO norms).  
It simplifies operations from **donor registration** to **blood issue**, and includes **government-compliant Excel/PDF reporting** for State and Central Health Authorities.

Developed using **.NET 9**, **SQL Server**, and **Stored Procedures** (without Entity Framework), this system provides a **secure, scalable, and fast** solution suitable for hospitals, NGOs, and regional blood centers.

---

## 🎯 Key Objectives

- Streamline and digitize daily blood center activities  
- Generate standardized government reports (Excel/PDF)  
- Provide transparency and audit trails for traceability  
- Offer a ready-to-use open-source project for developers, institutes, and healthcare organizations  

---

## ⚙️ Technology Stack

| Layer | Technology |
|-------|-------------|
| Backend | .NET 9 (ASP.NET Core MVC / Web API) |
| Database | Microsoft SQL Server |
| Data Access | ADO.NET / Dapper using Stored Procedures |
| Reporting | ClosedXML (Excel), Autovitia / QuestPDF (PDF) |
| Frontend | Razor Pages / Bootstrap (Responsive) |
| Deployment | IIS / Docker / Azure / On-premises |

---

## 🧩 Core Modules

### 1️⃣ User & Role Management
- Secure login, password policy, and audit trail  
- Role-based permissions (Super Admin, Center Admin, MO, Technician, etc.)  
- Access logs for accountability  

### 2️⃣ Donor Management
- Registration (voluntary/replacement/camp)  
- Donor health screening and deferral tracking  
- Medical examination forms and donation eligibility  
- Repeat donor reminders via SMS/Email  

### 3️⃣ Camp Management
- Camp planning, permissions, and scheduling  
- Donor attendance and collection tracking  
- Camp summary reports  

### 4️⃣ Blood Collection & Testing
- Blood bag entry with barcode support  
- Test results for HIV, HBsAg, HCV, VDRL, Malaria  
- Segregation of reactive/non-reactive units  
- Component preparation (RBC, Plasma, Platelets, Cryo)  

### 5️⃣ Inventory Management
- Blood group & component stock dashboard  
- Expiry and discard tracking  
- Inter-center stock transfers  
- Quarantine and destruction record management  

### 6️⃣ Hospital & Patient Requests
- Blood/component requests from hospitals  
- Crossmatching and compatibility validation  
- Issue, return, and replacement workflows  
- Receipt and billing generation  

### 7️⃣ Reports & Compliance
- Monthly/Quarterly/Annual summaries  
- Excel & PDF report generation for government upload  
- Internal dashboards and audit logs  

### 8️⃣ Master Data Management
- Lookup setup for blood groups, tests, hospitals, and staff  
- Central configuration of dropdown and metadata tables  

### 9️⃣ Notifications & Alerts
- SMS/Email reminders for donors and hospital requests  
- Camp and expiry alerts for users  

### 🔟 System Administration
- Database backup and restore  
- Application version info and changelog  
- API access key management (for future integrations)  

---

## 🧑‍💻 User Roles

| Role | Description |
|------|-------------|
| **Super Admin** | Overall system management and configuration |
| **Center Admin** | Manage specific blood center operations |
| **Medical Officer** | Authorize donor eligibility and test results |
| **Technician** | Manage collection, testing, and inventory updates |
| **Camp Coordinator** | Schedule and monitor donation camps |
| **Data Entry Operator** | Handle registrations and data input |
| **Hospital User** | Request and receive blood components |

---

## 📊 Workflow Summary

1. **Donor Registration** → Health Check → Donation Entry  
2. **Testing & Component Preparation** → Stock Update  
3. **Camp Management** → Donor and Collection Data  
4. **Hospital Request** → Crossmatch → Issue  
5. **Reports & Compliance** → Excel/PDF Export for Portals  

---

## ⚠️ Disclaimer

This application is provided **“as-is”**, without any warranties or guarantees.  
**Sadu Consultancy Services** and project contributors are **not responsible for any data loss, incorrect reports, or operational errors** arising from the use or modification of this software.  

Users must validate all operational workflows as per their organization and government compliance requirements.

---

## 📜 License

This project is licensed under the **Apache License 2.0**.  
You are free to **use, modify, and distribute** this software (with attribution).  

📄 [Read the full license text here](https://www.apache.org/licenses/LICENSE-2.0)

---

## 💬 Contact & Support

**Sadu Consultancy Services**  
📍 Pune, Maharashtra, India  
🌐 [www.saducs.com](http://www.saducs.com/bloodcenteros)  
📞 +91 9765975757  

---

## 🤝 Contribution Guidelines

We welcome contributions from developers, hospitals, and NGOs.  
Please open a **GitHub issue** or **pull request** for new features, bug reports, or suggestions.  

---

## 🌟 Vision

To build an open, transparent, and efficient **Blood Center Management Ecosystem**  
for India — promoting safety, traceability, and collaboration through technology.

---

© 2025 **Sadu Consultancy Services** — All rights reserved.  
Released under the **Apache License 2.0**.
