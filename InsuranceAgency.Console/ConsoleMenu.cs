using System.Globalization;
using InsuranceAgency.Core.Models;
using InsuranceAgency.Core.Services;

public class ConsoleMenu
{
    private readonly InsuranceAgencyService _agency;
    private readonly JsonFileService _fileService;

    public ConsoleMenu(InsuranceAgencyService agency, JsonFileService fileService)
    {
        _agency = agency;
        _fileService = fileService;
    }

    public void Run()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine("\n========== СТРАХОВА КОМПАНІЯ ========== ");
            Console.WriteLine("1. Додати клієнта");
            Console.WriteLine("2. Додати страхового агента");
            Console.WriteLine("3. Переглянути клієнтів");
            Console.WriteLine("4. Переглянути агентів");
            Console.WriteLine("5. Створити страховий поліс");
            Console.WriteLine("6. Переглянути поліси");
            Console.WriteLine("7. Зареєструвати страховий випадок");
            Console.WriteLine("8. Переглянути страхові випадки");
            Console.WriteLine("9. Змінити статус страхового випадку");
            Console.WriteLine("0. Зберегти та вийти");

            int choice = ReadInt("Ваш вибір: ", 0, 9);
            try
            {
                switch (choice)
                {
                    case 1: AddClient(); break;
                    case 2: AddAgent(); break;
                    case 3: ShowClients(); break;
                    case 4: ShowAgents(); break;
                    case 5: CreatePolicy(); break;
                    case 6: ShowPolicies(); break;
                    case 7: RegisterClaim(); break;
                    case 8: ShowClaims(); break;
                    case 9: ChangeClaimStatus(); break;
                    case 0:
                        _fileService.Save(_agency.Data);
                        Console.WriteLine("Дані збережено у JSON-файл. До побачення!");
                        running = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка: " + ex.Message);
            }
        }
    }

    private void AddClient()
    {
        var client = new Client
        {
            FirstName = ReadText("Ім'я: "),
            LastName = ReadText("Прізвище: "),
            Phone = ReadText("Телефон: "),
            Email = ReadText("Email: "),
            Address = ReadText("Адреса: "),
            BirthDate = ReadDate("Дата народження (рррр-мм-дд): ")
        };
        _agency.AddClient(client);
        Console.WriteLine("Клієнта додано. ID: " + client.Id);
    }

    private void AddAgent()
    {
        var agent = new Agent
        {
            FirstName = ReadText("Ім'я: "),
            LastName = ReadText("Прізвище: "),
            Phone = ReadText("Телефон: "),
            Email = ReadText("Email: "),
            EmployeeNumber = ReadText("Номер співробітника: "),
            CommissionPercent = ReadDecimal("Комісія агента (%): ", 0, 100)
        };

        _agency.AddAgent(agent);
        Console.WriteLine("Агента додано. ID: " + agent.Id);
    }

    private void ShowClients()
    {
        var clients = _agency.GetClients();
        if (clients.Count == 0) { Console.WriteLine("Клієнтів немає."); return; }
        foreach (var client in clients)
            Console.WriteLine($"{client.Id} | {client.FullName} | {client.Phone} | {client.Email}");
    }

    private void ShowAgents()
    {
        var agents = _agency.GetAgents();
        if (agents.Count == 0) { Console.WriteLine("Агентів немає."); return; }
        foreach (var agent in agents)
            Console.WriteLine($"{agent.Id} | {agent.FullName} | №{agent.EmployeeNumber} | Комісія: {agent.CommissionPercent}%");
    }

    private void CreatePolicy()
    {
        if (_agency.GetClients().Count == 0 || _agency.GetAgents().Count == 0)
        {
            Console.WriteLine("Спочатку додайте хоча б одного клієнта та агента.");
            return;
        }

        Console.WriteLine("Клієнти:");
        ShowClients();
        Guid clientId = ReadGuid("ID клієнта: ");
        Console.WriteLine("Агенти:");
        ShowAgents();
        Guid agentId = ReadGuid("ID агента: ");

        Console.WriteLine("Типи страхування: 1-Життя, 2-Медичне, 3-Майно, 4-Авто, 5-Туристичне");
        var type = (InsuranceType)ReadInt("Тип: ", 1, 5);
        decimal amount = ReadDecimal("Страхова сума (грн): ", 0.01m, 1000000000m);
        decimal coefficient = ReadDecimal("Коефіцієнт ризику (від 0.001 до 1): ", 0.001m, 1m);
        DateTime start = ReadDate("Дата початку (рррр-мм-дд): ");
        DateTime end = ReadDate("Дата завершення (рррр-мм-дд): ");

        InsurancePolicy policy = _agency.CreatePolicy(clientId, agentId, type, amount, coefficient, start, end);
        Console.WriteLine($"Поліс створено: {policy.PolicyNumber}. Вартість: {policy.Price:F2} грн.");
    }

    private void ShowPolicies()
    {
        var policies = _agency.GetPolicies();
        if (policies.Count == 0) { Console.WriteLine("Полісів немає."); return; }
        foreach (var p in policies)
            Console.WriteLine($"{p.Id} | {p.PolicyNumber} | {p.Type} | Страхова сума: {p.InsuredAmount:F2} | Ціна: {p.Price:F2} | Період: {p.StartDate:dd.MM.yyyy}–{p.EndDate:dd.MM.yyyy}");
    }

    private void RegisterClaim()
    {
        if (_agency.GetPolicies().Count == 0) { Console.WriteLine("Спочатку створіть страховий поліс."); return; }
        ShowPolicies();
        Guid policyId = ReadGuid("ID полісу: ");
        string description = ReadText("Опис страхового випадку: ");
        DateTime incidentDate = ReadDate("Дата випадку (рррр-мм-дд): ");
        decimal amount = ReadDecimal("Запитана сума виплати (грн): ", 0.01m, 1000000000m);

        Claim claim = _agency.RegisterClaim(policyId, description, incidentDate, amount);
        Console.WriteLine("Страховий випадок зареєстровано: " + claim.ClaimNumber);
    }

    private void ShowClaims()
    {
        var claims = _agency.GetClaims();
        if (claims.Count == 0) { Console.WriteLine("Страхових випадків немає."); return; }
        foreach (var c in claims)
            Console.WriteLine($"{c.Id} | {c.ClaimNumber} | Поліс: {c.PolicyId} | {c.RequestedAmount:F2} грн | {c.Status} | {c.Description}");
    }

    private void ChangeClaimStatus()
    {
        if (_agency.GetClaims().Count == 0) { Console.WriteLine("Немає страхових випадків для зміни статусу."); return; }
        ShowClaims();
        Guid id = ReadGuid("ID страхового випадку: ");
        Console.WriteLine("1-Подано, 2-Схвалено, 3-Відхилено, 4-Виплачено");
        ClaimStatus status = (ClaimStatus)ReadInt("Новий статус: ", 1, 4);
        _agency.SetClaimStatus(id, status);
        Console.WriteLine("Статус змінено.");
    }


    private static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();

            Console.WriteLine("Поле не може бути порожнім.");
        }
    }


    private static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max) return value;
            Console.WriteLine($"Введіть ціле число від {min} до {max}.");
        }
    }

    private static decimal ReadDecimal(string prompt, decimal min, decimal max)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim().Replace(',', '.') ?? string.Empty;
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value) && value >= min && value <= max) return value;
            Console.WriteLine($"Введіть число від {min} до {max}.");
        }
    }

    private static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine() ?? string.Empty;
            if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) return date.Date;
            Console.WriteLine("Введіть дату точно у форматі рррр-мм-дд, наприклад 2026-11-14.");
        }
    }

    private static Guid ReadGuid(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (Guid.TryParse(Console.ReadLine(), out Guid id)) return id;
            Console.WriteLine("Некоректний ID. Скопіюйте повний ID зі списку.");
        }
    }
}
