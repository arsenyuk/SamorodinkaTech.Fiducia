using System.Globalization;
using CsvHelper;

// Пример скрипта, демонстрирующего загрузку списка текущих ПАО в таблицу legal_entities.
// Требуется предварительно заполненный справочник ОКОПФ (минимум код 12247 — ПАО).
// Формат CSV: Name,ShortName,Inn,Ogrn

if (args.Length < 1)
{
    Console.WriteLine("Usage: dotnet run --project src/Tools/LegalEntitiesDemo <paos.csv>");
    return;
}

var path = args[0];
using var reader = new StreamReader(path);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var records = csv.GetRecords<dynamic>().ToList();

Console.WriteLine($"Importing {records.Count} PAO names with Okopf (ref_okopf.code=12247)...");

// Здесь вместо реальной БД — генерация SQL для наглядности
foreach (var rec in records)
{
    var name = (string)rec.Name;
    var shortName = (string?)rec.ShortName ?? string.Empty;
    var inn = (string?)rec.Inn ?? string.Empty;
    var ogrn = (string?)rec.Ogrn ?? string.Empty;
    // Демонстрация: если заранее известен UUID для ref_okopf (code=12247)
    var okopfId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1";
    var id = Guid.NewGuid();
    Console.WriteLine($"INSERT INTO legal_entities(id, name, short_name, inn, ogrn, okopf_id) VALUES ('{id}', '{name.Replace("'","''")}', '{shortName.Replace("'","''")}', '{inn}', '{ogrn}', '{okopfId}');");
}
