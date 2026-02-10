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

## 🖼️ Preview

<img width="1913" height="929" alt="Screenshot from 2026-02-10 19-01-10" src="https://github.com/user-attachments/assets/c2db35e7-08ed-4c03-90fd-9bafdf15d008" />
<img width="1913" height="929" alt="Screenshot from 2026-02-10 19-05-05" src="https://github.com/user-attachments/assets/0a991f70-cfbb-47a7-936a-9ef2966fdc58" />
<img width="1913" height="929" alt="Screenshot from 2026-02-10 19-08-25" src="https://github.com/user-attachments/assets/99304bac-3358-4059-a55c-0499e58a0d20" />

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
