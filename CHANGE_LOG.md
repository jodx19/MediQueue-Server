# CHANGE LOG — MediQueue EMR Clinic System

> كل تعديل أتعمل في المشروع مع التاريخ والوصف.  
> التنسيق: `YYYY-MM-DD` | `TYPE` | `الوصف`

---

## 2026-06-01

### التعديلات

| # | النوع | الملف | الوصف |
|---|-------|-------|--------|
| 1 | REFACTOR | `tailwind.config.js` | إعادة كتابة كاملة — تغيير الـ palette من green-teal إلى sky-blue (`mq-primary`) |
| 2 | REFACTOR | `src/styles.scss` | إعادة كتابة كاملة — 356 سطر تشمل glass, buttons, cards, badges, forms, tables, nav, bento, tabs, skeletons |
| 3 | REFACTOR | `src/styles/_tokens.scss` | تحديث CSS custom properties — sky-blue accent (#0284C7) + modern font stack |
| 4 | REFACTOR | `src/styles/globals.scss` | إعادة كتابة — root vars, scrollbar sky-blue, glass helpers |
| 5 | REFACTOR | `layout/shell/` | إعادة كتابة — collapsed sidebar (72px/240px), computed navItems حسب الـ role, SignalR status |
| 6 | REFACTOR | `features/dashboard/` | إعادة كتابة — bento grid layout مع 4 metric cards + chart.js |
| 7 | REFACTOR | `features/auth/login/` | إعادة كتابة — split layout: dark brand left + light form right |
| 8 | REFACTOR | `features/auth/patient-login/` | إعادة كتابة — split layout matching staff login |
| 9 | REFACTOR | `features/landing/` | إعادة كتابة — dark hero مع grid-bg + animated gradient text |
| 10 | REFACTOR | `features/patients/patient-list/` | إعادة كتابة — table-wrap مع th/td/tr + mock data |
| 11 | REFACTOR | `features/patients/patient-detail/` | إعادة كتابة — detail-grid مع info-card pairs |
| 12 | REFACTOR | `features/appointments/appointment-list/` | إعادة كتابة — 4 stat cards + table + status badges |
| 13 | REFACTOR | `features/doctors/doctor-list/` | إعادة كتابة — responsive card grid 3 cols |
| 14 | REFACTOR | `features/invoices/invoice-list/` | إعادة كتابة — 3 stat cards + table + InvoiceDto data |
| 15 | REFACTOR | `features/invoices/invoice-detail/` | إعادة كتابة — detail card + left sidebar forms |
| 16 | REFACTOR | `features/clinical-visits/my-queue/` | إعادة كتابة — page-header + queue-item + startVisit |
| 17 | REFACTOR | `features/clinical-visits/visit-detail/` | إعادة كتابة — tabbed SOAP/Vitals/Tx/Rx + auto-save |
| 18 | REFACTOR | `features/reports/` | إعادة كتابة — chart.js + date filter + Export buttons |
| 19 | REFACTOR | `features/patient-portal/` | إعادة كتابة — dashboard, appointments, invoices, records |
| 20 | REFACTOR | `layout/patient-shell/` | إعادة كتابة — dark header + tabbed sub-navigation |
| 21 | REFACTOR | `errors/` | إعادة كتابة — 404, 500, Forbidden (dark glass cards) |
| 22 | REFACTOR | `shared/notification-bell/` | إعادة كتابة — dropdownAnim + type-colored icons + timeAgo |
| 23 | REFACTOR | `shared/toast/` | إعادة كتابة — fixed top-right + slide-up animation |
| 24 | REFACTOR | `shared/forbidden/` | إعادة كتابة — dark glass + ShieldAlert + role-based redirect |
| 25 | FIX | `src/styles.scss` | إصلاح interpolation syntax — `#{"!important"}` → `px-3.5 py-1.5 text-xs` |
| 26 | FIX | `src/styles.scss` | إصلاح nesting — missing `}` لـ `@layer base` |
| 27 | FIX | `src/styles.scss` | إصلاح circular dependency — `@apply mq-input` في `.mq-input.error`, `.mq-textarea`, `.search-bar input` |
| 28 | FIX | `shell.component.ts` | تغيير import `filter` من `rxjs/operators` إلى حذفها (rxjs module resolution issue) |
| 29 | FIX | `shell.component.ts` | إصلاح type guard `e is ActivationEnd` مع `unknown` |
| 30 | FIX | `shell.component.ts` | إزالة `role === 'SuperAdmin'` (غير موجود في نوع UserSession) |
| 31 | FIX | `invoice-list.component.ts` | إصلاح `InvoiceStatus` enum (numeric) — إزالة مقارنة strings |
| 32 | BUILD | `node_modules/rxjs/index.d.ts` | إنشاء مؤقت (ثم حذف) — workaround لـ rxjs module resolution |
| 33 | FEATURE | `src/app/core/interceptors/refresh-token.interceptor.ts` | **إنشاء** — interceptor لـ JWT refresh مع pending queue + auto-retry |
| 34 | MODIFY | `src/app/app.config.ts` | إضافة `refreshTokenInterceptor` — الترتيب: `[authInterceptor, errorInterceptor, refreshTokenInterceptor]` |
| 35 | MODIFY | `src/app/core/auth/auth.service.ts` | إضافة `refreshToken` لـ `UserSession` + `readonly refreshToken` signal + `updateTokens()` method + حفظ `refreshToken` عند الـ login |
| 36 | DOC | `SYSTEM_REQUIREMENTS.md` | **إنشاء** — خارطة طريق 3 مراحل + ما يميز المنصة |
| 37 | DOC | `CHANGE_LOG.md` | **إنشاء** — سجل التعديلات |
| 38 | DOC | `FROZEN_FILES.md` | **إنشاء** — قواعد التجميد + المعمارية |
| 39 | ANALYSIS | `full final anaylsis.md` | **إنشاء** — تحليل كامل للباك اند والفرونت (11 قسم، 17 مشكلة، تقييم 8.2/10) |

---

## Glossary

| الاختصار | المعنى |
|----------|--------|
| REFACTOR | إعادة كتابة ملف كامل مع تغيير في التصميم أو المنطق |
| FIX | إصلاح مشكلة دون تغيير الوظيفة الأساسية |
| FEATURE | إضافة وظيفة جديدة |
| MODIFY | تعديل محدود على ملف موجود |
| BUILD | تغيير في إعدادات البناء أو dependencies |
| DOC | توثيق أو ملفات نصية |
| ANALYSIS | تقرير تحليلي |
