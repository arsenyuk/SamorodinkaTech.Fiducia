# ООО

- [Нетиповой устав](non-standard-charter.md)
- [Влияние параметров типового устава на бизнес-процессы](standard-charter-impact.md)
- [Устав ООО — ограничения на продажу долей](llc-charter-restrictions.md)
- [Покупка и продажа доли в ООО](llc-share-purchase-sale.md)
- [Выход участника из ООО](llc-participant-exit.md)
- [Переход неоплаченной доли к обществу](unpaid-share-transfer-to-society.md)
- [Назначение управляющего ИП (ст. 42 14-ФЗ)](llc-management-appointment.md)
- [Первичный ввод состава Совета директоров](board-setup-initial.md)
- [Заведение участников ООО в систему](llc-participant-enrollment.md)

## Сквозные тесты (E2E)

| Бизнес-процесс | E2E-тест | Статус |
|----------------|----------|--------|
| Авторизация | `US001_AuthorizationTests` | ✅ Реализован |
| Требования участника | `US020_ShareRequestTests` | ✅ Реализован |
| Каталог предоставленных документов | `US021_DocumentCatalogTests` | ✅ Реализован |
| Участники ООО (список) | `US023_ParticipantTests` | ✅ Реализован |
| Типовой устав | `E2E_StandardCharterTests` | ✅ Реализован |
| Нетиповой устав | `E2E_NonStandardCharterTests` | ✅ Реализован |
| Первичный ввод состава СД | `E2E_BoardSetupTests` | ✅ Реализован |
| Генеральный директор | `E2E_GeneralDirectorTests` | ✅ Реализован |
| ЕДИН-интеграция | `E2E_EdinIntegrationTests` | ✅ Реализован |
| ЕДИН-сценарии | `E2E_EdinScenarioTests` | ✅ Реализован |
| Управление пользователями | `E2E_UserManagementTests` | ✅ Реализован |
| Требование о созыве ВОСУ | `E2E_VosuDemandTests` | ✅ Реализован |

Подробнее: [`docs/e2e-tests.md`](../../e2e-tests.md)
