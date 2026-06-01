# MediQueue EMR — خارطة الطريق (Roadmap)

> **الرؤية:** منصة SaaS احترافية لإدارة العيادات الطبية  
> **التفريق:** AI-first · Arabic-first · Offline-capable · White-label  
> **التقنية:** .NET 9 Clean Architecture · Angular 18 Standalone · SQL Server · Redis

---

## المرحلة الأولى — إصلاح الأساس (Foundation)

> **الهدف:** تحويل الـ MVP إلى منتج متكامل ومستقر  
> **المدة المقدرة:** 2–3 أسابيع

### 1.1 Refresh Token Interceptor
- [x] Functional interceptor يلتقط 401
- [ ] معالجة الـ concurrent requests (pending queue)
- [ ] إعادة المحاولة بعد التجديد
- [ ] تسجيل الخروج إذا فشل التجديد
- [ ] تجاهل endpoints المصادقة لمنع infinite loops

### 1.2 ApiResponse<T> Wrapper موحد
- [ ] استخدام `response.data` بشكل ثابت في كل الـ components
- [ ] إلغاء التوقع المباشر لـ response (الـ components تتوقع DTO مباشر حالياً)
- [ ] إنشاء `response unwrapper` service أو تعديل generated client

### 1.3 NSwag يشمل كل الـ Controllers
- [ ] إضافة `UsersController` إلى الـ NSwag config
- [ ] إضافة `AttachmentsController` إلى الـ NSwag config
- [ ] تحديث `nswag.json` ليشمل كل routes
- [ ] إعادة توليد `mediqueue-api.ts`

### 1.4 Reactive Forms
- [ ] استبدال `ngModel` بـ `FormGroup`/`FormControl` في:
  - VisitDetailComponent (SOAP notes)
  - InvoiceDetailComponent (discount, payment)
  - PatientRegisterComponent
  - BookAppointmentComponent
- [ ] FluentValidation-matching validators في الفرونت

### 1.5 حذف المكونات المكررة
- [ ] `ToastService` (حذف — استعمال NotificationService)
- [ ] `patient-portal/book-appointment/` (دمج مع `patient-portal/book/`)
- [ ] `ClinicSettingsComponent` (دمج مع `SettingsComponent`)
- [ ] Mock components: `AdminDashboardComponent`, `AppointmentsComponent`, `InvoicesComponent`, `ClinicalVisitsDashboardComponent`

### 1.6 InvoiceStatus Enum Mapper
- [ ] إنشاء Pipe/method لتحويل `InvoiceStatus` (numeric enum) إلى string مفهوم
- [ ] تصحيح الـ comparisons في `invoice-list` و `invoice-detail`

### 1.7 Unit Tests الأساسية
- [ ] AuthService tests
- [ ] Guards tests (auth, role)
- [ ] Interceptors tests (auth, error, refresh-token)
- [ ] Coverage ≥ 20%

---

## المرحلة الثانية — اكتمال الوظائف (Feature Complete)

> **الهدف:** تغطية كاملة لكل endpoints الباك اند مع واجهة مستخدم متقنة  
> **المدة المقدرة:** 4–6 أسابيع

### 2.1 ClinicalVisit Tabs كاملة
- [ ] تبويبات: SOAP · Vital Signs · Diagnoses · Procedures · Labs · Imaging · Referrals · Prescriptions
- [ ] كل تبويب له form إنشاء + عرض + تعديل
- [ ] Auto-save مع debounce (موجود حالياً للـ SOAP فقط)

### 2.2 Attachments Upload UI
- [ ] Drag & drop file upload
- [ ] ربط مع `AttachmentsController`
- [ ] عرض attachments في patient-detail و visit-detail
- [ ] تحديد نوع الملف (Lab report, X-ray, Prescription, etc.)

### 2.3 Doctor CRUD في Frontend
- [ ] Create/Edit doctor form
- [ ] إدارة Working Shifts
- [ ] إدارة Qualifications
- [ ] Unavailable periods

### 2.4 Invoice Create Workflow
- [ ] واجهة إنشاء فاتورة جديدة من Appointment
- [ ] إضافة items (خدمات/منتجات)
- [ ] طباعة الفاتورة
- [ ] إرسال الفاتورة للـ Patient

### 2.5 Caching Strategy (Redis)
- [ ] تفعيل `ICacheService` (Redis) في الـ queries
- [ ] Cache invalidation عند الـ commands
- [ ] Distributed cache للـ session (اختياري)

### 2.6 E2E Tests أساسية
- [ ] Playwright/Cypress للأجزاء الحرجة:
  - Login flow
  - Patient registration
  - Appointment booking
  - Invoice workflow
- [ ] Coverage ≥ 40%

### 2.7 Error Handling شامل
- [ ] Error pages لكل الأخطاء (Network error, Rate limit, etc.)
- [ ] Retry strategy للـ failed requests
- [ ] User-friendly error messages بالعربية

### 2.8 Performance Optimization
- [ ] Lazy loading images
- [ ] Virtual scroll للجداول الكبيرة
- [ ] Bundling optimization
- [ ] Lighthouse score ≥ 85

---

## المرحلة الثالثة — SaaS احترافي متميز (Enterprise)

> **الهدف:** منصة جاهزة للمنافسة في السوق  
> **المدة المقدرة:** 3–4 أشهر

### 3.1 Multi-Tenant Architecture
- [ ] **Tenant isolation:** Separate schema per clinic
- [ ] **Tenant resolution:** Domain → TenantId mapping  
- [ ] **Shared infrastructure:** Redis, Hangfire queues لكل tenant
- [ ] **Admin panel:** إدارة الـ tenants، الاشتراكات، الحدود

### 3.2 Subscription + Billing Engine
- [ ] **Subscription tiers:**
  - Free: 50 patient/month, 1 doctor
  - Professional: 500 patient/month, 5 doctors
  - Enterprise: Unlimited
- [ ] **Payment gateway:** Stripe/Paymob integration
- [ ] **ميزات حسب الـ tier:**
  - Free: Basic EMR + Appointments
  - Professional: + AI SOAP, Reports, WhatsApp
  - Enterprise: + White-label, API access, Dedicated support

### 3.3 AI Clinical Assistant
- [ ] **AI SOAP Notes:** تحويل الكلام (voice → text → SOAP) — توفير 30 دقيقة لكل كشف
  - Speech-to-text API
  - LLM summarization → SOAP structure
  - Integration مع ClinicalVisit
- [ ] **Smart Scheduling بـ ML:**
  - Analyse patient no-show history
  - Predict optimal appointment slots
  - Double-booking prevention مع probability scores
- [ ] **Drug Interaction Checker (Real-time):**
  - RxNorm / DrugBank API integration
  - Check أثناء كتابة الـ prescription
  - Alert: Contraindications, Allergies, Duplicate therapy

### 3.4 Advanced Analytics Dashboard
- [ ] **Clinic KPIs:**
  - Revenue trends + forecasting
  - Appointment fill rate
  - Patient retention
  - Doctor performance
- [ ] **Export:** PDF, Excel, Power BI integration
- [ ] **Custom reports builder**

### 3.5 Mobile PWA للمرضى
- [ ] **Offline-capable:**
  - Service worker caching
  - IndexedDB للبيانات المحلية
  - Sync عند الاتصال
- [ ] **Progressive Web App:**
  - Install prompt
  - Push notifications
  - Camera access (upload prescriptions)
- [ ] **ما يدعمه:**
  - View appointments (offline)
  - Receive notifications
  - Quick booking

### 3.6 White-Label لكل عيادة
- [ ] **Custom domain:** clinicname.mediqueue.com
- [ ] **Branding:** Logo, colors, fonts من لوحة التحكم
- [ ] **Theming engine:** Dynamic CSS variables
- [ ] **Patient portal:** بهوية العيادة
- [ ] **Email templates:** Customizable بالعلامة التجارية للعيادة

### 3.7 WhatsApp Reminders (مدمجة)
- [ ] **Appointment reminders:** 24h + 2h قبل الموعد
- [ ] **Invoice notifications:** فاتورة جديدة، تذكير بالدفع
- [ ] **Prescription delivery:** وصفة طبية عبر WhatsApp
- [ ] **Two-way:** الرد لتأكيد/إلغاء الموعد
- [ ] **WhatsApp Business API** integration

### 3.8 Arabic-First UX
- [ ] **RTL support:** Full mirror layout
- [ ] **Localization:** i18n (ar + en)
- [ ] **Hijri dates:** التقويم الهجري بجانب الميلادي
- [ ] **Content:** كل المحتوى بالعربية أولاً، الإنجليزية ثانياً
- [ ] **Voice:** الأوامر الصوتية بالعربية (المساعد الذكي)

---

## ما يميز MediQueue عن أي منصة موجودة

### تميز تقني فريد
| الميزة | القيمة للمستخدم | الأثر |
|--------|-----------------|-------|
| AI SOAP Notes | يوفر 30 دقيقة على الدكتور لكل كشف | زيادة إنتاجية 3× |
| Smart Scheduling بـ ML | يملأ الفراغات تلقائياً | زيادة إيرادات 20–30% |
| Drug Interaction Checker | يمنع الأخطاء الدوائية | أمان للمريض + حماية قانونية |
| Patient Self-Service Portal | يقلل الاتصالات بالعيادة 60% | توفير وقت الـ staff |

### تميز تجاري
| الميزة | المشكلة التي تحلها | القيمة |
|--------|--------------------|--------|
| White-label | كل عيادة تريد هوية مستقلة | ولاء للعلامة التجارية |
| Offline-capable | مناطق ضعيفة الإنترنت بالوطن العربي | وصول لأكثر من 50% من السوق |
| WhatsApp Reminders | 30% no-show rate → <5% | زيادة إيرادات + تحسين جدول المواعيد |
| Arabic-First UX | 400 مليون مستخدم عربي محروم من EMR عربي حقيقي | سوق غير مشبع بالكامل |

---

## الحكم النهائي

> الأساس قوي جداً — Clean Architecture + DDD + CQRS هي أفضل مما يبنيه معظم الفرق المحترفة. المشكلة مش في التصميم، هي في **الاكتمال**.  
> **المرحلة الأولى** بتحولك من MVP إلى منتج.  
> **المرحلة الثانية** بتحولك من منتج إلى نظام متكامل.  
> **المرحلة الثالثة** بتحولك من نظام إلى منصة تستحق subscription شهري حقيقي.
