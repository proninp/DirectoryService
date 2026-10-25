# DS-20: SoftDeletionBackgroundService — оставшиеся замечания

## Статус

| #  | Замечание                                         | Статус           |
|----|---------------------------------------------------|------------------|
| 1  | Опции не биндятся (позиционный record)            | ✅ Сделано       |
| 2  | Нет валидации опций                               | ✅ Сделано       |
| 3  | Утечка scope на каждой итерации                   | ✅ Сделано       |
| 4  | `DateTime.UtcNow` в валидаторе фиксируется один раз | ✅ Сделано     |
| 5  | Результат прогона игнорируется                    | ✅ Сделано       |
| 6  | Имя файла не совпадает с именем класса            | ✅ Сделано       |
| 7  | Опциональные улучшения                            | ✅ Сделано (мелочь с `LogContext`, см. п.7) |
| 8  | `Program` глотает исключения старта хоста         | ❌ Не сделано    |
| 9  | Глобальный bootstrap-логгер Serilog ломает параллельный старт хостов | ⚠️ Обойдено в тестах |

---

## Осталось сделать

## 7. [Остаток] Старое имя в `LogContext`

Опечатка `DeleteOlderThen` → `DeleteOlderThan` исправлена в команде, хендлере и валидаторе, но
осталась в имени свойства анонимного объекта логирования:

```csharp
public object? LogContext => new { DeleteOlderThen = DeleteOlderThan, BatchSize };
```

В логах (Seq) поле по-прежнему называется `DeleteOlderThen`. Достаточно
`new { DeleteOlderThan, BatchSize }`.

## 8. `Program` глотает исключения старта хоста

`Program.cs` оборачивает весь запуск в `try/catch (Exception)`:

```csharp
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
```

Исключение не пробрасывается дальше, поэтому:
- **В проде:** при невалидном конфиге `ValidateOnStart` бросает `OptionsValidationException`, но
  процесс пишет Fatal в лог и завершается с кодом **0**. Docker/k8s считают запуск успешным, а
  restart policy `on-failure` не сработает.
- **В тестах:** через `WebApplicationFactory` нельзя проверить падение при старте. Исключение
  перехватывается внутри `Main`, и наружу доходит только
  `ObjectDisposedException: Cannot access a disposed object. Object name: 'IServiceProvider'`.
  Поэтому `SoftDeletionOptionsRegistrationTest` проверяет `ValidateOnStart` на отдельном
  минимальном `Host` с `AddSoftDeletionCleanup`, а не через `TestWebFactory`.

Исправление — пробросить исключение после логирования. Тогда код выхода будет ненулевым:

```csharp
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
```

`HostAbortedException` бросает `WebApplicationFactory`, чтобы остановить `Main` после сборки
хоста. Его логировать как Fatal и пробрасывать не нужно.

## 9. Глобальный bootstrap-логгер Serilog ломает параллельный старт хостов

`Program.cs` присваивает глобальный `Log.Logger = new LoggerConfiguration()...CreateBootstrapLogger()`,
а `AddSerilog((sp, lc) => ...)` при сборке хоста «замораживает» этот `ReloadableLogger`. Все
`TestWebFactory` работают в одном процессе, и xUnit поднимает тестовые классы параллельно. Если
`Main` двух хостов перекрываются, второй замораживает уже замороженный логгер:

```
System.InvalidOperationException: The logger is already frozen.
```

Из-за п.8 исключение проглатывается, и тестовый класс падает целиком с
`InvalidOperationException: The entry point exited without ever building an IHost`. Падает
случайный класс (`CreatePositionTest`, `DeleteLocationsTest`, `TopLocationsTest` и т.д.). Проблема
была и раньше, но с ростом числа тестовых классов стала воспроизводиться почти в каждом прогоне.

**Обход в тестах (сделано).** `TestWebFactory.InitializeAsync` запускает хосты по очереди через
статический `SemaphoreSlim` вокруг первого обращения к `Services`. Контейнеры PostgreSQL
по-прежнему поднимаются параллельно. Проверено 5 прогонами подряд: 113/113.

**Исправление в коде (не сделано).** Убрать зависимость от глобального состояния, например:
- не использовать `CreateBootstrapLogger()`, а настраивать Serilog только через
  `builder.Services.AddSerilog(...)`, без `ReloadableLogger`;
- или создавать bootstrap-логгер только вне тестового окружения (не в `Testing`).

После исправления семафор в `TestWebFactory` можно убрать.

---

## Сделано

- **1. Биндинг опций.** `SoftDeletionCleanupRetention` переделан из позиционного record в record
  со свойствами и конструктором без параметров, `MissingMethodException` при старте больше нет.
  Мелочи тоже закрыты: дефолт `Timeout` исправлен на `TimeSpan.FromSeconds(20)` (совпадает с
  `appsettings.json`), `required` убран, `set` заменён на `init`.
- **2. Валидация опций.** Написан `SoftDeletionCleanupRetentionValidator`
  (`Interval > 0`, `Timeout > 0`, `Timeout < Interval`, `RetentionDays > 0`, `BatchSize > 0`) и
  зарегистрирован в `AddSoftDeletionCleanup` как `IValidateOptions<SoftDeletionCleanupRetention>`.
  Регистрация опций переведена на `AddOptions<>().Bind(...).ValidateOnStart()`, в сервисе
  обрабатывается `OptionsValidationException` при hot reload. Покрыто тестами:
  - `SoftDeletionCleanupRetentionValidatorTest` — правила валидатора;
  - `SoftDeletionOptionsRegistrationTest` — валидатор зарегистрирован, хост с невалидной секцией
    падает на `StartAsync` с `OptionsValidationException`;
  - `Purge_Should_Continue_After_Invalid_Options_Reload` — сервис переживает невалидный конфиг
    после hot reload (через тот же валидатор из DI).

  Мелочь, которая осталась: первое чтение `_retentionOption.CurrentValue` и создание таймера
  стоят вне `try`. С `ValidateOnStart` это безопасно, но если конфиг станет невалидным между
  стартом хоста и первым вызовом `ExecuteAsync`, исключение уронит хост, а не попадёт в `catch`.
- **3. Утечка scope.** Теперь `await using var scope = _serviceScopeFactory.CreateAsyncScope();`
  внутри `try`.
- **4. Валидатор и время.** `PurgeDeletionValidator` использует `.LessThan(_ => timeProvider.GetUtcNow().UtcDateTime)`,
  время берётся в момент валидации. Лишний `.NotNull()` убран. Ложное падение при
  `RetentionDays = 0` больше неактуально и потому, что `RetentionDays` теперь должен быть > 0.
- **5. Результат прогона.** `IsFailure` логируется как Warning с ошибками валидации, успешный
  результат логируется через `LogPurgeSummary` (Information для `Completed`, Warning для
  `Cancelled`/`Failed`) с полной статистикой по наборам.
- **6. Имя файла.** `SofDeletionBackgroundService.cs` переименован в
  `SoftDeletionBackgroundService.cs` (переименование в индексе git).
- **7. Опционально.**
  - Первый прогон сразу при старте: цикл переписан на `do { ... } while (await timer.WaitForNextTickAsync(...))`.
  - Hot reload `Interval`: на каждой итерации `timer.Period` обновляется, если `Interval` изменился.
  - `DeleteOlderThen` → `DeleteOlderThan` в команде, хендлере и валидаторе (кроме `LogContext`, см. п.7 выше).
  - `TimeProvider` внедрён в сервис и валидатор (`TryAddSingleton(TimeProvider.System)` в
    `Application/Registration.cs`), расчёт порога покрыт тестом
    `Purge_Should_Calculate_Threshold_From_TimeProvider` с `FixedTimeProvider`.
