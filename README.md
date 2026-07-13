<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/5b65c297-b5bc-426f-9542-e37d06a79054" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/ad485e89-5bdf-4356-b631-15457d09e1ef" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/659846da-17ed-429d-97ed-878dcc77b8be" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/b2a8ad5f-e59f-46cb-994c-f28fbf4c5e12" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/38236a33-5949-429e-93d9-0a6ac136cbe2" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/b82c7944-6637-4ae6-9473-c48c633e28ad" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/bedd82de-6446-4691-9842-cfd85e7c868c" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/a585fe69-0ebb-4d22-af32-0bb6cb5038e0" />
<img width="250" height="440" alt="image" src="https://github.com/user-attachments/assets/decdba57-b6bc-404f-8cd2-78e42b87243a" />

# VoiceX

<p align="center">
  <img width="200" height="120" alt="image" src="https://github.com/user-attachments/assets/c165c753-5e6a-4909-b38e-00258f4631a3"/>
  <br>
  <b>An advanced, lightweight, and high-performance cross-platform SIP softphone.</b>
  <br>
  <sub>A modern, feature-rich alternative to MicroSIP built for seamless VoIP communication.</sub>
</p>

<p align="center">
  <img src="https://img.shields.io/github/license/FokshaDmitry/VoiceX?style=flat-square&color=blue" alt="License">
  <img src="https://img.shields.io/github/v/release/FokshaDmitry/VoiceX?style=flat-square&color=success" alt="Latest Release">
  <img src="https://img.shields.io/badge/.NET-8.0-blueviolet?style=flat-square" alt=".NET Version">
  <img src="https://img.shields.io/badge/Engine-PJSIP%20%2F%20PJSUA2-orange?style=flat-square" alt="PJSIP Engine">
</p>

---

## 📌 Overview

**VoiceX** is a next-generation VoIP softphone designed for users who appreciate the lightning-fast, resource-efficient nature of MicroSIP but require modern interfaces, cross-platform flexibility, and enhanced feature sets. 

Powered by the robust, industry-standard **PJSIP (PJSUA2)** engine and leveraging the performance of modern **.NET**, VoiceX ensures crystal-clear audio, low latency, and rock-solid connection stability with any standard SIP-based PBX (such as Asterisk, FreePBX, 3CX, or cloud VoIP providers).

---

## ✨ Key Features

- **🚀 Ultra-Lightweight & Fast:** Minimal memory footprint and instant startup times, just like MicroSIP.
- **🎧 Crystal Clear Audio:** Advanced media pipeline with support for high-quality codecs (Opus, G.711, G.722), Acoustic Echo Cancellation (AEC), and Adaptive Jitter Buffer.
- **🎨 Modern UX/UI:** A clean, intuitive, and highly responsive user interface designed for both everyday business tasks and intensive call-center environments.
- **🔄 Standalone Auto-Updates:** Features an integrated background update system communicating with a custom ASP.NET WebAPI microservice, ensuring you always run the latest, most secure version automatically.
- **🛡️ Enterprise Ready:** Fully compatible with standard SIP extensions, authentication schemas, and advanced call control (Hold, Mute, Attended/Blind Transfer).
- **📋 Core Telephony Essentials:** Quick-access dialer, localized configuration, precise call history logs, and system tray integration.

---

## 🏗️ Architecture

VoiceX uses a decoupled architecture to separate high-level presentation logic from sensitive audio and network management:

1. **Presentation Layer:** Built using scalable modern UI frameworks to deliver a smooth user experience across target desktop environments.
2. **Service Abstraction:** A managed C# (.NET) service layer managing account lifecycles, connection statuses, background workers, and thread orchestration.
3. **Core Telecom Engine:** Native bindings to **PJSUA2** ensuring high performance for SIP registration, signaling, and secure RTP media transport.

---

## 🚀 Getting Started

### Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or higher.
- A valid SIP/VoIP account from your provider or local PBX.

### Installation

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/FokshaDmitry/VoiceX.git](https://github.com/FokshaDmitry/VoiceX.git)
   cd VoiceX
