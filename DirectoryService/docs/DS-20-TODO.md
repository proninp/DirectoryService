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
| 10 | Purge отделов падает на FK, если у отдела есть дочерние | ❌ Не сделано    |

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

## 10. Purge отделов падает на FK, если у отдела есть дочерние

`fk_department_parent` создан с `ON DELETE RESTRICT` (миграция `AddedDepartmentIdentifierUniqueness`).
Если у давно удалённого отдела есть дочерний, `ExecuteDelete` в `DeletedRecordsCleanupService`
падает с 23503 на весь батч. Прогон завершается с `Outcome = Failed`, наборы локаций и позиций
пропускаются. «Застрявший» отдел выбирается `Take(batchSize)` каждый раз, поэтому очистка
ломается навсегда.

Сценарий воспроизводится через API: `DeleteDepartmentHandler` проверяет связи с локациями и
позициями, но не проверяет дочерние отделы.

### Правило

Отдел удаляется, только если в таблице **нет ни одной** его дочерней строки, включая удалённые.
Проверки «нет активных детей» недостаточно, потому что FK блокирует удаление при любом
существующем ребёнке:

| Дочерний отдел                   | Удаление родителя                        |
|----------------------------------|------------------------------------------|
| активный                         | FK-ошибка                                |
| удалён, срок хранения не вышел   | FK-ошибка, ребёнок ещё в таблице         |
| удалён давно, сам подлежит purge | можно, но после ребёнка или вместе с ним |

Проверять только прямых детей тоже мало. Пример: P и C удалены давно, внук G активен. Прямая
проверка пропустит P (C сам подлежит purge), но C останется из-за G, и удаление P упадёт.

### Решение: удалять «листья», пока удаляется хоть что-то

За один батч удаляются только отделы без дочерних строк:

```csharp
context.Departments
    .IgnoreQueryFilters()
    .Where(d => !d.IsActive && d.DeletedAt < olderThan)
    .Where(d => !context.Departments.IgnoreQueryFilters().Any(c => c.ParentId == d.Id))
    .Take(batchSize)
    .ExecuteDeleteAsync(ct);
```

Цикл продолжается, пока `deletedCount > 0`. Сейчас он останавливается при
`deletedCount < batchSize`, а для отделов этого мало: после удаления листьев их родители сами
становятся листьями.

Что это даёт:
- **Транзитивность без рекурсии.** Ветка P → C → G с активным G не удаляется. Ветка, удалённая
  целиком, удаляется снизу вверх за несколько итераций.
- **Ни одного FK-нарушения.** Родитель и ребёнок не попадают в одну команду, поэтому не важно,
  когда PostgreSQL проверяет `RESTRICT`, и не нужна сортировка по `Depth`.
- **Не зависит от `Path`.** Используется только `ParentId`.
- **Цена:** одна лишняя пустая итерация в конце и число итераций порядка «глубина × батчи».

Индекс `ix_departments_parent_id` уже есть (EF создал его для FK), подзапрос `Any` по нему
быстрый.

Отвергнутые варианты:
- `NOT EXISTS` по потомкам через `path LIKE p.path || '/%'` + `ORDER BY depth DESC`. Меньше
  запросов, но зависит от корректности `Path` после перемещений, и на границе батчей всё равно
  нужна сортировка.
- Рекурсивный CTE сырым SQL. Самый точный вариант, но это сырой SQL в EF-сервисе, и его сложнее
  поддерживать.

### Изменения по слоям

1. **`IDeletedRecordsCleanupService`** (Application): отдельный метод
   `Task<int> PurgeDepartmentsAsync(DateTime olderThan, int batchSize, CancellationToken ct = default)`.
   Он явно показывает, что у отделов другие правила, вместо скрытой подмены по типу внутри
   `PurgeAsync<Department>`. `PurgeAsync<TEntity>` остаётся для `Location` и `Position`.
   Альтернатива: параметр `Expression<Func<TEntity, bool>>? extraFilter`, но тогда отдельно
   придётся менять и условие выхода из цикла.
2. **`DeletedRecordsCleanupService`** (Infrastructure.Postgres): реализовать метод с фильтром
   листьев и циклом `while (deletedCount > 0 && !ct.IsCancellationRequested)`. Попутно
   переименовать параметр `olderThen` → `olderThan`.
3. **`PurgeDeletedRecordsHandler`**: заменить `PurgeAsync<Department>` на `PurgeDepartmentsAsync`.
   Набор отделов перестанет падать с `Failed`, локации и позиции перестанут пропускаться.
4. **Опционально, наблюдаемость:** после purge посчитать отделы, которые подлежали удалению, но
   остались из-за детей (тот же фильтр с `Any` вместо `!Any`). Добавить `DepartmentsBlocked` в
   `PurgeDeletedRecordsResult` и выводить в `LogPurgeSummary` (Warning при `> 0`). Иначе такие
   записи будут копиться незаметно.

### Тесты

| # | Данные                                                         | Ожидание                                                         |
|---|----------------------------------------------------------------|------------------------------------------------------------------|
| 1 | старый удалённый родитель, активный ребёнок                    | оба остаются; `Outcome = Completed`; локации и позиции очищены   |
| 2 | старый удалённый родитель, недавно удалённый ребёнок           | оба остаются                                                     |
| 3 | родитель и ребёнок, оба старые удалённые                       | оба удалены за один прогон                                       |
| 4 | три уровня, все старые, `BatchSize = 1`                        | все удалены (проверка цикла до нуля)                             |
| 5 | P и C старые удалённые, G активный                             | все три остаются                                                 |
| 6 | старый родитель, один активный и один старый удалённый ребёнок | удалённый ребёнок удалён, родитель остаётся                      |
| 7 | регрессия: повторный прогон после сценария 1                   | ничего не падает, родитель на месте                              |

### Открытый вопрос

`DeleteDepartmentHandler` позволяет мягко удалить родителя при живых детях, и после этого
изменения такой родитель будет жить вечно. Варианты:
- запретить в хендлере (Conflict `department.has.children`);
- удалять каскадно всю ветку.

От выбора зависит, будет ли `DepartmentsBlocked > 0` нормой или признаком проблемы.

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
