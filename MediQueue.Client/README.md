# 🏥 MediQueue EMR - Frontend
### Modern, Intelligent, and Sleek Clinic Management System

[![Angular](https://img.shields.io/badge/Angular-18-DD0031?style=for-the-badge&logo=angular)](https://angular.io/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.4-38B2AC?style=for-the-badge&logo=tailwind-css)](https://tailwindcss.com/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.5-3178C6?style=for-the-badge&logo=typescript)](https://www.typescriptlang.org/)

MediQueue EMR is a state-of-the-art Electronic Medical Record system designed for modern clinics. It features a premium, Apple-inspired user interface that prioritizes speed, clarity, and user experience.

---

## ✨ Key Features

- **🚀 Smart Dashboard**: Real-time analytics, revenue charts, and daily schedules at a glance.
- **📁 Advanced EMR**: Comprehensive patient records including medical history, allergies, and chronic conditions.
- **🩺 Clinical Visits**: Streamlined SOAP note recording, vital signs tracking, and digital prescriptions.
- **📅 Appointment Engine**: Drag-and-drop scheduling with status tracking (Checked-in, In-Progress, Completed).
- **💳 Financial Suite**: Integrated invoicing, payment tracking, and revenue reporting.
- **📱 Patient Portal**: Self-service appointment booking and medical history access.
- **🔔 Real-time Notifications**: Live updates for new appointments and system alerts via SignalR.

---

## 🎨 Design Philosophy

- **Apple-Inspired Aesthetic**: Clean lines, subtle glassmorphism, and smooth micro-animations.
- **Dark Mode Ready**: Sophisticated color palettes optimized for clinical environments.
- **Responsive & Fluid**: Optimized for desktops, tablets, and mobile browsers.

---

## 🛠️ Tech Stack & Architecture

- **Core**: Angular 18 (Standalone Components, Signals).
- **Styling**: Tailwind CSS for utility-first responsive design.
- **Icons**: Lucide Angular for consistent, lightweight iconography.
- **State Management**: Angular Signals for reactive and performant state updates.
- **API Client**: Auto-generated TypeSafe NSwag client for seamless backend integration.
- **Real-time**: @microsoft/signalr for live data synchronization.

---

## 🚀 Getting Started

### Prerequisites
- Node.js (v18+)
- Angular CLI (`npm install -g @angular/cli`)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/jodx19/MediQueue-Client.git
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Run the development server:
   ```bash
   npm start
   ```
4. Navigate to `http://localhost:4200/`.

---

## 📂 Project Structure

```text
src/app/
├── core/         # Interceptors, Guards, API Services, Auth
├── shared/       # Reusable components, pipes, directives
├── layout/       # Shell, Sidebar, Navbar
└── features/     # Feature modules (Admin, Doctor, Patients, etc.)
```

---

## 📜 License
Developed as part of the ITI Graduation Project. 

---
Developed with ❤️ by [Mahmoud Mostafa](https://github.com/jodx19)
