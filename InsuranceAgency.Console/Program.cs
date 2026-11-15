using System.Text;
using System.Text.Json;
using InsuranceAgency.Core.Services;

// Реєструємо підтримку Windows-кодувань.
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Для введення використовуємо Windows-1251.
Console.InputEncoding = Encoding.GetEncoding(1251);

// Для виведення залишаємо UTF-8.
Console.OutputEncoding = Encoding.UTF8;

string dataPath = Path.Combine(AppContext.BaseDirectory, "insurance-data.json");

var fileService = new JsonFileService(dataPath);
AppData data;

try
{
    data = fileService.Load();
    Console.WriteLine("Дані успішно завантажено.");
}
catch (JsonException)
{
    Console.WriteLine("Файл даних пошкоджений або має неправильний формат. Буде створено нове сховище.");
    data = new AppData();
}
catch (IOException ex)
{
    Console.WriteLine("Не вдалося прочитати файл: " + ex.Message);
    data = new AppData();
}

var agency = new InsuranceAgencyService(data);
var menu = new ConsoleMenu(agency, fileService);
menu.Run();
