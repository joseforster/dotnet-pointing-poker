# Pointing Poker

A simple planning poker web app for agile teams, featuring real-time voting, secure authentication, and session management.

🔗 Live demo: https://pointingpoker.joseforster.com

---

## 🚀 Features

- Real-time estimation with live updates
- Create and join sessions
- Reveal results simultaneously
- Watch secondary sessions (e.g., QA voting)
- Kick unwanted users
- Secure authentication

---

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core, SignalR  
- **Frontend:** Razor Pages, Bootstrap 5  
- **Hosting:** Docker, Cloudflare Tunnel  
- **Authentication:** Google OAuth, Cloudflare OTP
- **Authorization:** ASP.NET Core Cookie Authentication
- **Logging:** Serilog

---

## 📦 Installation

```bash
git clone https://github.com/joseforster/dotnet-pointing-poker
cd dotnet-pointing-poker
dotnet run
```

Or, pulling from the docker hub (in the example below I used port 3000)

```bash
docker pull jmforster/pointing-poker:latest
docker run -d -p 3000:8080 --name pointing-poker jmforster/pointing-poker:latest
```
