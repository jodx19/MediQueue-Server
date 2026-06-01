# Frozen Files — Do Not Modify

> الملفات التالية تعتبر **مجمدة** — لا يُسمح بتعديلها أبداً إلا إذا تم ذكرها صراحة في الـ prompt.  
> هذه القاعدة تحمي integrity النظام وتمنع كسر الـ generated client والـ architecture.

---

## Absolute Freeze — ممنوع التعديل مطلقاً

| الملف | السبب |
|-------|--------|
| `src/app/core/api/mediqueue-api.ts` | NSwag generated client — أي تعديل يُمسح عند إعادة التوليد. هو المصدر الوحيد الموثوق لأنواع الـ API. |
| `tailwind.config.js` | يتحكم بالـ design system كله. أي تغيير هنا يأثر على كل الصفحات. |
| `angular.json` | إعدادات الـ build — لا يُمس إلا عند إضافة dependency أو تغيير output path. |
| `nswag.json` | إعدادات توليد الـ API client — تعديله فقط عند إضافة/تغيير Controller في الباك. |
| `package.json` | dependencies — لا تُعدل يدوياً إلا عند إضافة package جديد (وبعد `npm install`). |

---

## Modify Only When Explicitly Listed in a Prompt

> هذه الملفات يمكن تعديلها فقط عندما يكون الطلب يذكرها بالاسم.

| الملف | مسموح به |
|-------|----------|
| `src/app/core/auth/auth.service.ts` | إضافة signals/helpers للـ auth flow (مثل `updateTokens`, `refreshToken` getter) |
| `src/app/app.config.ts` | إضافة/إزالة Interceptors أو Providers |
| `src/app/app.routes.ts` | إضافة/تعديل Routes أو Guards |
| `src/app/core/interceptors/*.ts` | إنشاء Interceptors جديدة (لا تعديل الموجودة) |
| `src/environments/environment.ts` | تغيير API Base URL أو إعدادات البيئة |

---

## Architecture Rules — Never Break

### Frontend (Angular 18)
| القاعدة | الشرح |
|---------|--------|
| **Standalone only** | لا يُسمح بإنشاء NgModule. كل Component/Directive/Pipe يكون `standalone: true`. |
| **No NgRx** | إدارة الحالة فقط بـ Signals — ممنوع NgRx، Redux، أو أي state management library. |
| **Functional interceptors** | كل Interceptor يكون `HttpInterceptorFn` — ممنوع class-based interceptors. |
| **Functional guards** | كل Guard يكون `CanActivateFn` — ممنوع class-based guards. |
| **Sky-blue design system** | الـ palette هو `mq-primary` (sky-blue). ممنوع تغيير الألوان الأساسية. |
| **NSwag = source of truth** | أنواع الـ API تأتي فقط من `mediqueue-api.ts`. لا تُعرف أنواع مكررة في الـ components. |
| **Lazy loading** | كل الـ routes تستخدم `loadComponent()` — ممنوع الـ eager loading للـ features. |

### Backend (.NET 9 Clean Architecture)
| القاعدة | الشرح |
|---------|--------|
| **No cross-layer imports** | الـ Domain لا يعرف Application. الـ Application لا يعرف Infrastructure. وهكذا. |
| **CQRS for all writes** | أي عملية كتابة تمر عبر `ICommand`/`ICommandHandler` (MediatR). |
| **Repository pattern** | الوصول للبيانات فقط عبر Repositories — ممنوع استخدام DbContext مباشرة خارج Infrastructure. |
| **Result<T> everywhere** | كل Handler يرجع `Result<T>` — ممنوع return entities مباشرة. |
| **Domain events for side effects** | الـ notification، الـ scheduling، الـ email كلها تكون Domain events. |
| **Soft delete** | كل الأنتيتيز ترث `BaseEntity` مع `IsDeleted` — ممنوع `DELETE FROM`. |

### API Design
| القاعدة | الشرح |
|---------|--------|
| **Unified response** | كل endpoint يرجع `ApiResponse<T>` — ممنوع return models مباشرة. |
| **JWT only** | Authentication عبر JWT Bearer — ممنوع cookies أو session-based auth. |
| **Role-based auth** | كل endpoint له `[Authorize(Roles=...)]` — ممنوع الـ authorization اليدوي. |

### Database
| القاعدة | الشرح |
|---------|--------|
| **EF Core migrations** | أي تغيير في الـ schema يكون عبر `dotnet ef migrations add` — ممنوع التعديل المباشر. |
| **Soft delete** | Global query filter `IsDeleted == false` — ممنوع الحذف الفعلي. |
| **Audit fields** | `CreatedAt`/`CreatedBy`/`UpdatedAt`/`UpdatedBy` في كل جدول. |

---

## استثناءات — بقرار مسبب

في حال كان التعديل ضرورياً على ملف مجمّد، يجب:
1. ذكر السبب في الـ prompt
2. الحصول على موافقة صريحة
3. تسجيل التعديل في `CHANGE_LOG.md`
4. تعويض التعديل في المصدر الأصلي إذا كان ممكناً (مثل إعادة توليد NSwag)

> **المبدأ:** الجليد يحمي من الكسر — لكنه لا يمنع التطور المخطط له.
