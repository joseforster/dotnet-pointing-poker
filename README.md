# Pointing Poker

A simple planning poker web app for agile teams, built with ASP.NET Core, SignalR, and Bootstrap. Link Below:
(https://pointingpoker.joseforster.com/)

---

## 🚀 Features

- Real-time communication using SignalR  
- Create and join sessions  
- Select estimated hours and reveal results  

---

## 🖼️ Preview

<img width="1920" height="1200" alt="Screenshot from 2025-12-21 11-31-43" src="https://github.com/user-attachments/assets/12db0a65-c911-4ee7-8248-fcd0b0cfee0b" />
<img width="1920" height="1200" alt="Screenshot from 2025-12-22 17-21-49" src="https://github.com/user-attachments/assets/8fa9483e-4afe-4afe-8146-f81c92c7e039" />

---

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core, SignalR  
- **Frontend:** Razor, Bootstrap 5  
- **Language:** C#  
- **Hosting:** Docker and Cloudflare tunnel as reverse proxy

---

## 📦 Installation

```bash
git clone https://github.com/joseforster/dotnet-pointing-poker
cd dotnet-pointing-poker
dotnet run

Or, pulling from the public docker hub (in the example below I used port 3000)

```bash
docker pull jmforster/pointing-poker:latest
docker run -d -p 3000:8080 --name pointing-poker jmforster/pointing-poker:latest
