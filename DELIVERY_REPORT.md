# تقرير التسليم — دورة العمل 2026-09-21
**الفرع:** `main` — **النطاق:** توحيد الواجهة الحكومية + فجوات API + تغطية اختبارات خالصة
**التحقق النهائي:** `dotnet test HospitalProject.slnx` ← 35/35 خضراء، 0 أخطاء، 110 تحذيرات (خط الأساس، لا جديد)

---

## 1. الاختبارات (جديد)
| الملف | المحتوى |
|---|---|
| `Hospital.Tests/Hospital.Tests.csproj` | xUnit 2.9.2 + Moq 4.20.70 + FluentAssertions 6.12.0، `net8.0`، مضاف لـ`HospitalProject.slnx` |
| `Hospital.Tests/ShiftCalculatorTests.cs` | 13 حالة Modulo-4 (أمامي/خلفي/سنة كبيسة/مسح ±1000 يوم) |
| `Hospital.Tests/LeaveBalanceCalculatorTests.cs` | خصم/فرق مدة/استرداد + دورة حياة كاملة مطابقة للكنترولر |
| `Hospital.Tests/PagedResultTests.cs` | عقد الترقيم الذي تعتمد عليه كل شاشات Desktop |
| `Hospital.API/Services/ShiftCalculator.cs` | رياضيات الدوران النقية (مستخرجة من `ShiftService` بسلوك مطابق) |
| `Hospital.API/Services/LeaveBalanceCalculator.cs` | حسابات الرصيد النقية (`LeavesController` يفوّض إليها) |

## 2. إصلاحات API
- `TransferLogController.cs` — أُضيف `PUT {id}` (Admin,Manager؛ فحص FK؛ مزامنة الموظف فقط إذا كان السجل الأحدث). **بلا DELETE عمداً:** النموذج بلا `isDeleted` والحذف الفعلي ممنوع وسجل التنقلات أرشيف تاريخي.
- `NightShiftTeamsController.cs` — `PUT` أصبح يستقبل `NightShiftTeamDto` (مطابق لحمولة Desktop) + `[Authorize(Roles="Admin")]` + فحص وجود المشرف + منع ازدواج المشرف (نفس قاعدة `ShiftsController`) + رسائل عربية.
- **خلل مثبت ومُصلح:** `Leaves/Absents/AuditLogs/TransferLog` كانت تُرجع كائنات مجهولة بلا `TotalCount/PageSize` فينكسر `TotalPages` المحسوب في Desktop — وحّدتها على `PagedResult<T>` كالنمط Canonical في `EmployeesController.cs:57` (السلك يحتفظ بـ`TotalPages`؛ صفر تغيير في Desktop).

## 3. التوحيد الحكومي للواجهة
- جديد: `Hospital.Desktop/Themes/GovernmentalBrushes.xaml` (لوحة `#0F172A`/`#1E293B`/`#2563EB`/`#1D4ED8`/`#F8FAFC`/`#E2E8F0` + حالات أخضر/كهرماني/أحمر داكن + خط Segoe UI/Tahoma) و`ButtonStyles.xaml` (4 أنماط) و`DataGridStyles.xaml` (شبكة + حقول + Validation) — مدمجة في `App.xaml` (أُزيلت 3 أنماط ميتة بعد التحقق من صفر استخدام).
- `MainWindow.xaml` → كحلي + تمييز ملكي؛ `EmployeesView/EmployeeFormView/LoginView` هُجّرت من Fluent؛ كل الشاشات الـ21 بلا أي hex قديم (مدقق بـgrep؛ الرماديات المحايدة بقيت inline عمداً).
- **قرار موثق:** أُبقيت أيقونات Segoe MDL2 — خط نظام مضمون في هدف `net8.0-windows`، فالتحويل لمسارات متجهة = تغيير بلا فائدة.

## 4. الوثائق
- `PROJECT_MAP.md` — أرقام 2026-09-21 (35 اختباراً، القواميس، الحاسبات، PUT التنقلات، توحيد PagedResult).
- `TASK_TREE.md` — TST-01/TST-02/UI-01-03/SH-01/QA-02/QA-03 ← `[DONE]`؛ الباقي: `QA-01 Pass B` (تحذيرات nullable، تغيير واسع بلا أثر سلوكي)، `SH-02` (إثبات طباعة يحتاج تطبيقاً يعمل)، اختبارات `AuthController` الحية (تحتاج API+DB+JWT). العمل تم على `main` مباشرة (وكيل واحد متسلسل — لا تعارض يستدعي فروعاً).

## 5. أوامر إعادة التحقق
```bash
dotnet test HospitalProject.slnx        # 35/35
dotnet build HospitalProject.slnx --no-incremental  # 0 errors
git status --short                       # نظيف بعد هذا الالتزام
```

---

## الدورة 2 (2026-09-21) — Pass B + إعادة تصميم مرئية + لوحة قيادة
- **Pass B:** التحذيرات **110 ← 4** (0 أخطاء). الإصلاحات: `RelayCommand` و`OnPropertyChanged` و`ApiService` (nullable مركزياً)، توقيعات nullable لكل VMs (الحراس كانت موجودة)، `!` في API والمحولات، حذف `ex` غير المستخدمة. الباقي 4 = CS8981 (أسماء Migrations مقبولة — إعادة تسميتها = churn في EF ممنوع).
- **إعادة تصميم مرئية حقيقية** (الدورة السابقة كانت توحيد رموز بلا فرق مرئي): بطاقات بيضاء موحدة + شارة أيقونات ملونة + بطاقة خفر كحلية + أشرطة توزيع الدوام.
- **لوحة قيادة جديدة:** `DashboardViewModel.cs` (7 تجميعات متوازية محمية: أعداد + أحدث + فريق اليوم عبر `Shifts/calculate`) + `DashboardView.xaml` (5 بطاقات KPI، خفر اليوم، توزيع صباحي/مسائي، أحدث الإجازات/الغيابات، أكبر الأقسام، إجراءات سريعة تتنقل للأقسام) + `DataTemplate` في `App.xaml` + `MainViewModel` يعرضها ابتدائياً.
- **ملاحظة تشغيلية:** أثناء العمل كان `Hospital.API` يعمل (PID 21912) ويقفل DLLs ففشل النسخ — أُعيد البناء بعد إغلاقه. لرؤية التغييرات: أعد بناء `Hospital.Desktop` وشغّل من جديد.

---

## الدورة 3 (2026-09-21) — overhaul حكومي صارم (Steps 1–4)
- **Step 1:** `Colors.xaml` (7 فرش + RowAlternate/RowSelected/AccentDark/PrimaryAccentColor) + `Typography.xaml` (AppFontFamily/Header1/Header2/Body/Small/MonoFontFamily) + `Styles.xaml` (Primary/Secondary صريحة CR=3؛ TextBox/ComboBox/DataGrid/TextBlock ضمنية؛ ترويسة كحلية/بيضاء؛ تناوب `#F1F5F9`؛ تحديد `#DBEAFE`؛ RTL).
- **Step 2:** الشل على المفاتيح Canonical + فاصل حدودي + `Header2` للشعار (الكحلي `#0F172A` → `PrimaryDark` المواصفة؛ hover/active مميزة).
- **Step 3:** **صفر hex في الشاشات والشل** (مدقق grep: ألوان→canonical، حالات→GovSuccess/Warning/Danger المركزية، خطوط→AppFontFamily/Mono، أيقونات MDL2 محفوظة كنظام أيقونات).
- **Step 4:** كل الأزرار الرئيسية Primary/Secondary؛ العناوين Header1/Header2؛ الترويسات الفاتحة المحلية محذوفة (الكحلية Canonical تسود)؛ الأنماط المحلية الميتة محذوفة (`ModernBtn/ModernButtonStyle/ActionBtn/Gov*Button`) وملفا `ButtonStyles/DataGridStyles` محذوفان؛ `PrimaryBtn/SecondaryBtn` أُعيد تأسيسها `BasedOn` مع الاحتفاظ بالارتفاعات؛ `GovernmentalBrushes` مُقلّم لعائلات الحالات فقط.
- **استثناءات موثقة:** مقاييس `Margin/Padding/FontSize` المتناثرة بقيت (لا سلم تباعد في المواصفة)؛ أرقام KPI العرضية بأحجامها؛ `Transparent` الكلمية؛ Segoe MDL2 للأيقونات (خط نظام).
- **تحقق:** حل كامل 0 أخطاء (4×CS8981 مقبولة) + اختبارات 35/35. أخطاء أُصلحت أثناء العمل: تكرار `<Grid>` في الشل، سطر `Border` محذوف في Leaves، `AppFontFamily` محذوف سهواً (أُعيد فوراً).
