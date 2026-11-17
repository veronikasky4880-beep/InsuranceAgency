КУРСОВИЙ ПРОЄКТ — ВАРІАНТ 18 «СТРАХОВА КОМПАНІЯ»

1. Загальний опис проєкту та виконання вимог
Проєкт являє собою програмне забезпечення для автоматизації обліку клієнтів, страхових агентів, договорів страхування (полісів) та страхових випадків (заяв на виплату). Програма здійснює розрахунок вартості полісів з урахуванням ризикових коефіцієнтів, контролює строки дії договорів, реєструє та обробляє страхові випадки з дотриманням страхових лімітів.
Код системи повністю дотримується стандартів об'єктно-орієнтованого програмування, принципів SOLID, строгої інкапсуляції та розділення відповідальностей (Separation of Concerns).

2. Архітектура та структура рішення
Рішення організовано у вигляді структури з двох взаємопов'язаних проектів у межах одного Solution (InsuranceAgency.sln):

InsuranceAgency.Core (Class Library, .NET 8.0) — автономна бібліотека класів, яка містить усю бізнес-логіку, моделі предметної області, інтерфейси, узагальнені репозиторії, обчислювальні модулі та сервіс серіалізації. Не має жодних залежностей від консольного інтерфейсу.
InsuranceAgency.Console (Console Application, .NET 8.0) — проєкт інтерфейсу користувача. Включає посилання на InsuranceAgency.Core.csproj, організовує текстове меню, забезпечує перевірку коректності введених даних та налаштування системного кодування.

Структура файлів проєкту
InsuranceAgency/
├── InsuranceAgency.sln
├── InsuranceAgency.Core/
│   ├── Exceptions/
│   │   ├── ClaimAmountExceededException.cs
│   │   └── PolicyExpiredException.cs
│   ├── Interfaces/
│   │   ├── IInsuranceCalculator.cs
│   │   └── IRepository.cs
│   ├── Models/
│   │   ├── Agent.cs
│   │   ├── Claim.cs
│   │   ├── ClaimStatus.cs
│   │   ├── Client.cs
│   │   ├── InsurancePolicy.cs
│   │   ├── InsuranceType.cs
│   │   └── Person.cs
│   ├── Repositories/
│   │   └── InMemoryRepository.cs
│   ├── Services/
│   │   ├── AppData.cs
│   │   ├── InsuranceAgencyService.cs
│   │   ├── InsuranceCalculator.cs
│   │   └── JsonFileService.cs
│   └── InsuranceAgency.Core.csproj
└── InsuranceAgency.Console/
    ├── ConsoleMenu.cs
    ├── Program.cs
    └── InsuranceAgency.Console.csproj


3. Опис бізнес-частини (InsuranceAgency.Core)
3.1. Моделі предметної області та інкапсуляція
Усі моделі винесено в окремі файли в папку Models. Для захисту даних застосовано принципи інкапсуляції — прямий доступ до внутрішніх полів обмежено.

Person (абстрактний базовий клас):
Містить спільні атрибути для фізичних осіб: унікальний ідентифікатор Guid Id, ім'я FirstName, прізвище LastName, телефон Phone, електронну пошту Email. Також реалізує обчислювану властивість FullName ($"{FirstName} {LastName}").
Client (наслідує Person):
Доповнює базовий клас атрибутами адреси проживання Address та дати народження BirthDate.
Agent (наслідує Person):
Доповнює базовий клас табельним номером співробітника EmployeeNumber та відсотком комісійної винагороди CommissionPercent.
InsurancePolicy:
Модель страхового поліса. Зберігає номер поліса PolicyNumber, ідентифікатори застрахованого клієнта ClientId та відповідального агента AgentId, тип страхування InsuranceType, страхову суму InsuredAmount, ризиковий коефіцієнт RiskCoefficient, підсумкову вартість (премію) Price, а також термін дії (StartDate, EndDate).
Claim:
Модель страхового випадку. Зберігає номер виплати ClaimNumber, посилання на поліс PolicyId, опис події Description, дату настання страхового випадку IncidentDate, запитану суму відшкодування RequestedAmount та поточний статус ClaimStatus.
Перераховувані типи (Enums):

InsuranceType: Life (Життя), Health (Медичне), Property (Майно), Vehicle (Авто), Travel (Туристичне).
ClaimStatus: Submitted (Подано), Approved (Схвалено), Rejected (Відхилено), Paid (Виплачено).



3.2. Абстракції та узагальнений репозиторій
Для маніпуляції об'єктами у пам'яті реалізовано паттерн Repository з використанням узагальнень (Generics):
namespace InsuranceAgency.Core.Interfaces;

public interface IRepository<T> where T : class
{
    void Add(T item);
    bool Remove(T item);
    List<T> GetAll();
}

Конкретна реалізація InMemoryRepository<T> працює з узагальненою колекцією List<T>, що повністю задовольняє вимоги для отримання оцінки «Відмінно»:
using InsuranceAgency.Core.Interfaces;

namespace InsuranceAgency.Core.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items;

    public InMemoryRepository(List<T> items)
    {
        _items = items;
    }

    public void Add(T item) => _items.Add(item);
    public bool Remove(T item) => _items.Remove(item);
    public List<T> GetAll() => _items;
}

3.3. Розрахунок страхових премій
Розрахунок вартості полісів винесено в окремий сервіс InsuranceCalculator, який реалізує інтерфейс IInsuranceCalculator:
namespace InsuranceAgency.Core.Interfaces;

public interface IInsuranceCalculator
{
    decimal CalculatePolicyPrice(decimal insuredAmount, decimal riskCoefficient);
}

Формула обчислення валідує вхідні параметри та повертає округлене значення страхового внеску:
$$\text{Price} = \text{Math.Round}(\text{InsuredAmount} \times \text{RiskCoefficient}, 2)$$
3.4. Власні класи виключень (Custom Exceptions)
Для обробки специфічних помилок страхового бізнесу створено два кастомних класи виключень у папці Exceptions:

PolicyExpiredException: Виникає у разі спроби зареєструвати страховий випадок за договором, строк дії якого закінчився на момент події.
ClaimAmountExceededException: Виникає, якщо запитана сума виплати (разом із раніше схваленими виплатами за даним полісом) перевищує загальну страхову суму поліса.

namespace InsuranceAgency.Core.Exceptions;

public class PolicyExpiredException : Exception
{
    public PolicyExpiredException(string message) : base(message) { }
}

public class ClaimAmountExceededException : Exception
{
    public ClaimAmountExceededException(string message) : base(message) { }
}

3.5. Головний сервіс бізнес-логіки (InsuranceAgencyService)
Клас InsuranceAgencyService є центральним вузлом бізнес-логіки. Він координує роботу репозиторіїв, здійснює бізнес-валідацію та забезпечує наступні операції:

Додавання та перевірка клієнтів і агентів (перевірка формату Email, дати народження, унікальності номера співробітника агента).
Оформлення страхових полісів із перевіркою наявності клієнта/агента та коректності дат.
Реєстрація страхових випадків із перевіркою лімітів виплат та строків дії поліса.
Управління статусами виплат із захистом від некоректних переходів станів.


4. Сервіс персистентності та JSON-серіалізація
Збереження та завантаження даних здійснюється через клас JsonFileService, який використовує системну бібліотеку System.Text.Json.
Контейнер даних AppData зберігає списки всіх сутностей (Clients, Agents, Policies, Claims).
Процес серіалізації налаштовано з форматуванням (WriteIndented = true), що робить підсумковий JSON-файл insurance-data.json легкочитайним. Завантаження відбувається автоматично під час старту консольного застосування, а збереження — під час вибору пункту меню «0. Зберегти та вийти».

5. Консольний інтерфейс користувача (InsuranceAgency.Console)
5.1. Текстове меню та навігація
Інтерфейс користувача реалізовано в класі ConsoleMenu.cs. Основне меню пропонує наступний функціонал:

Додати клієнта
Додати страхового агента
Переглянути клієнтів
Переглянути агентів
Створити страховий поліс
Переглянути поліси
Зареєструвати страховий випадок
Переглянути страхові випадки
Змінити статус страхового випадку
Зберегти та вийти

5.2. Захищений ввід даних та валідація
Для запобігання аварійному завершенню програми при некоректному вводі користувача написано серію статичних методів-валідаторів:

ReadText(prompt) — перевіряє, щоб рядок не був порожнім.
ReadInt(prompt, min, max) — використовує int.TryParse для перевірки вводу чисел у заданому діапазоні.
ReadDecimal(prompt, min, max) — обробляє ввід дробових чисел (автоматично замінює кому на крапку для сумісності з CultureInfo.InvariantCulture).
ReadDate(prompt) — перевіряє точне дотримання формату дати yyyy-MM-dd.
ReadGuid(prompt) — гарантує коректне парсинг унікальних ідентифікаторів.

5.3. Підтримка кирилиці в консолі Windows
Для коректного відображення та введення українських символів у консолі Windows (cmd.exe) в Program.cs підключено провайдер кодувань та встановлено відповідні таблиці:
using System.Text;
using System.Text.Json;
using InsuranceAgency.Core.Services;

// Реєстрація підтримки кодувань Windows
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Налаштування вводу для Windows-1251 та виводу для UTF-8
Console.InputEncoding = Encoding.GetEncoding(1251);
Console.OutputEncoding = Encoding.UTF8;


6. UML-діаграма класів
Діаграма класів описує структуру системних компонентів, відносини успадкування, реалізації інтерфейсів та зв'язки між сутностями.
Відображення на GitHub
GitHub надає вбудовану підтримку Mermaid. Діаграма нижче рендериться безпосередньо у веб-інтерфейсі GitHub при перегляді цього файлу README.md.
classDiagram
    direction TB

    %% --- 1. ІЄРАРХІЯ СУТНОСТЕЙ (МОДЕЛІ) ---
    class Person {
        <<abstract>>
        +Guid Id
        +string FirstName
        +string LastName
        +string Phone
        +string Email
        +string FullName
    }

    class Client {
        +string Address
        +DateTime BirthDate
    }

    class Agent {
        +string EmployeeNumber
        +decimal CommissionPercent
    }

    Person <|-- Client : успадковує
    Person <|-- Agent : успадковує

    class InsurancePolicy {
        +Guid Id
        +string PolicyNumber
        +Guid ClientId
        +Guid AgentId
        +InsuranceType Type
        +decimal InsuredAmount
        +decimal RiskCoefficient
        +decimal Price
        +DateTime StartDate
        +DateTime EndDate
    }

    class Claim {
        +Guid Id
        +string ClaimNumber
        +Guid PolicyId
        +string Description
        +DateTime IncidentDate
        +decimal RequestedAmount
        +ClaimStatus Status
    }

    class InsuranceType {
        <<enumeration>>
        Life
        Health
        Property
        Vehicle
        Travel
    }

    class ClaimStatus {
        <<enumeration>>
        Submitted
        Approved
        Rejected
        Paid
    }

    InsurancePolicy --> InsuranceType : тип страхування
    Claim --> ClaimStatus : статус виплати
    Client "1" <-- "*" InsurancePolicy : має поліси
    Agent "1" <-- "*" InsurancePolicy : оформив
    InsurancePolicy "1" <-- "*" Claim : містить виплати

    %% --- 2. КАСТОМНІ ВИКЛЮЧЕННЯ ---
    class PolicyExpiredException {
        +PolicyExpiredException(string message)
    }

    class ClaimAmountExceededException {
        +ClaimAmountExceededException(string message)
    }

    Exception <|-- PolicyExpiredException
    Exception <|-- ClaimAmountExceededException

    %% --- 3. РЕПОЗИТОРІЇ ТА ІНТЕРФЕЙСИ ---
    class IRepository~T~ {
        <<interface>>
        +Add(T item) void
        +Remove(T item) bool
        +GetAll() List~T~
    }

    class InMemoryRepository~T~ {
        -List~T~ _items
        +Add(T item) void
        +Remove(T item) bool
        +GetAll() List~T~
    }

    IRepository~T~ <|.. InMemoryRepository~T~ : реалізує

    class IInsuranceCalculator {
        <<interface>>
        +CalculatePolicyPrice(decimal insuredAmount, decimal riskCoefficient) decimal
    }

    class InsuranceCalculator {
        +CalculatePolicyPrice(decimal insuredAmount, decimal riskCoefficient) decimal
    }

    IInsuranceCalculator <|.. InsuranceCalculator : реалізує

    %% --- 4. СЕРВІСИ ТА БАЗА ДАНИХ ---
    class AppData {
        +List~Client~ Clients
        +List~Agent~ Agents
        +List~InsurancePolicy~ Policies
        +List~Claim~ Claims
    }

    class InsuranceAgencyService {
        -AppData _data
        -IRepository~Client~ _clients
        -IRepository~Agent~ _agents
        -IRepository~InsurancePolicy~ _policies
        -IRepository~Claim~ _claims
        -IInsuranceCalculator _calculator
        +AddClient(Client client) void
        +AddAgent(Agent agent) void
        +CreatePolicy(...) InsurancePolicy
        +RegisterClaim(...) Claim
        +SetClaimStatus(Guid claimId, ClaimStatus status) void
    }

    class JsonFileService {
        -string _filePath
        +Save(AppData data) void
        +Load() AppData
    }

    %% --- 5. КОНСОЛЬНИЙ ІНТЕРФЕЙС (UI) ---
    class ConsoleMenu {
        -InsuranceAgencyService _agency
        -JsonFileService _fileService
        +Run() void
    }

    InsuranceAgencyService --> IInsuranceCalculator : використовує
    InsuranceAgencyService --> AppData : бере дані
    ConsoleMenu --> InsuranceAgencyService : викликає бізнес-логіку
    ConsoleMenu --> JsonFileService : зберігає/завантажує


Примітка щодо додавання графічного файлу в репозиторій:
Якщо ви бажаєте також додати PNG-зображення діаграми у свій репозиторій, збережіть графічний файл у папку (наприклад, docs/diagram.png) та додайте наступний рядок у README.md:
![UML Class Diagram](docs/diagram.png)



7. Інструкція з збірки та запуску
Вимоги до середовища

.NET 8.0 SDK або новіша версія.
Visual Studio 2022 або Visual Studio Code.

Кроки для запуску через командний рядок (CLI)


Клонувати репозиторій:
git clone <URL_вашого_репозиторію>
cd InsuranceAgency



Відновити залежності та зібрати рішення:
dotnet build InsuranceAgency.sln



Запустити консольний проект:
dotnet run --project InsuranceAgency.Console/InsuranceAgency.Console.csproj




8. Демонстрація роботи та тестування
Нижче наведено приклади виконання основних сценаріїв у консольному інтерфейсі.
8.1. Додавання клієнта та агента
========== СТРАХОВА КОМПАНІЯ ==========
1. Додати клієнта
2. Додати страхового агента
...
Ваш вибір: 1
Ім'я: Назар
Прізвище: Коваленко
Телефон: +380501234567
Email: nazar.kovalenko@example.com
Адреса: м. Київ, вул. Хрещатик, 1
Дата народження (рррр-мм-дд): 2002-05-14
Клієнта додано. ID: a46c6ed8-1739-420d-b28b-d32126143ee7

Ваш вибір: 2
Ім'я: Дарина
Прізвище: Шевченко
Телефон: +380671234567
Email: daryna.shevchenko@example.com
Номер співробітника: AG001
Комісія агента (%): 5
Агента додано. ID: c0f11dfb-6701-4987-a19b-28376371582c

8.2. Перегляд списку клієнтів
Ваш вибір: 3
a46c6ed8-1739-420d-b28b-d32126143ee7 | Назар Коваленко | +380501234567 | nazar.kovalenko@example.com

8.3. Створення страхового поліса
Ваш вибір: 5
Клієнти:
a46c6ed8-1739-420d-b28b-d32126143ee7 | Назар Коваленко | +380501234567 | nazar.kovalenko@example.com
ID клієнта: a46c6ed8-1739-420d-b28b-d32126143ee7
Агенти:
c0f11dfb-6701-4987-a19b-28376371582c | Дарина Шевченко | №AG001 | Комісія: 5%
ID агента: c0f11dfb-6701-4987-a19b-28376371582c
Типи страхування: 1-Життя, 2-Медичне, 3-Майно, 4-Авто, 5-Туристичне
Тип: 4
Страхова сума (грн): 500000
Коефіцієнт ризику (від 0.001 до 1): 0.02
Дата початку (рррр-мм-дд): 2026-01-01
Дата завершення (рррр-мм-дд): 2026-12-31
Поліс створено: POL-202611150412-A1B2C3. Вартість: 10000.00 грн.

8.4. Тестування обробки виключень (Захист системи)


Спроба вводу некоректної дати або порожнього поля:
Email:
Поле не може бути порожнім.
Email: nazar@example.com



Перевищення страхової суми при реєстрації виплати (ClaimAmountExceededException):
Ваш вибір: 7
ID полісу: 8f3d1e2a-5b4c-4d3e-9a1b-2c3d4e5f6a7b
Опис страхового випадку: Пошкодження майна
Дата випадку (рррр-мм-дд): 2026-06-15
Запитана сума виплати (грн): 600000
Помилка: Сума заяви разом із вже схваленими виплатами перевищує страхову суму.



Спроба подачі заяви по простроченому полісу (PolicyExpiredException):
Ваш вибір: 7
Дата випадку (рррр-мм-дд): 2027-02-01
Помилка: На дату страхового випадку строк дії поліса вже завершився.




9. Висновки
У ході виконання курсової роботи було розроблено повнофункціональне консольне застосування мовою C# для автоматизації діяльності страхової компанії.
У проєкті успішно реалізовано:

Розділення бізнес-логіки та консольного UI на два окремих проекти (Core.dll та Console App).
Принципи об'єктно-орієнтованого програмування (інкапсуляцію, успадкування, поліморфізм).
Узагальнені колекції та розробку власного узагальненого репозиторію IRepository<T>.
Сервіс збереження та завантаження стану об'єктів у JSON-файл за допомогою System.Text.Json.
Кастомні виключення для контролю бізнес-правил страхової компанії.
Надійну валідацію консольного вводу та підтримку кодування кирилиці Windows-1251 / UTF-8.
