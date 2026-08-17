# Лицензированные регистраторы Российской Федерации

> Регистратор — профессиональный участник рынка ценных бумаг, имеющий лицензию Банка России на осуществление деятельности по ведению реестра владельцев ценных бумаг. С 1 октября 2014 года все акционерные общества **обязаны** передать ведение реестра регистратору ([ст. 44 208-ФЗ](article-44.md)).

---

## Основные сведения

| Параметр | Значение |
|----------|----------|
| Правовая основа | [ст. 8](article-39fz-registrar.md), [8.6-1](article-39fz-registrar.md) 39-ФЗ |
| Лицензия | Бессрочная, выдаётся Банком России |
| Реестр лицензий | Ведётся Банком России на сайте [cbr.ru](https://cbr.ru) |
| VidID (вид деятельности) | `4` — «Деятельность по ведению реестра владельцев ценных бумаг» |

---

## Поиск регистраторов через SOAP-сервис ЦБ РФ

> Подробнее о сервисе: [article-cbr-finorg-api.md](article-cbr-finorg-api.md)

Регистраторы ищутся методом `Search` по **виду деятельности `VidID=4`** (не по `FoType`, т.к. `PT` слишком широкий):

```
POST https://cbr.ru/FO_ZoomWS/FinOrg.asmx
SOAPAction: http://web.cbr.ru/Search

<tns:Search>
  <tns:Name></tns:Name>          <!-- пусто = все -->
  <tns:Status>Active</tns:Status> <!-- только действующие -->
  <tns:FoType></tns:FoType>       <!-- пусто = все типы -->
  <tns:VidID>4</tns:VidID>        <!-- ← ключевой фильтр -->
  <tns:OKATO>-1</tns:OKATO>
  <tns:page>0</tns:page>
</tns:Search>
```

Полная информация о конкретном регистраторе — через `GetFullInfoByINN`.

---

## Реестр лицензированных регистраторов (по данным ЦБ РФ)

> По состоянию на август 2026 года. Источник: SOAP-сервис [FinOrg.asmx](https://cbr.ru/FO_ZoomWS/FinOrg.asmx), VidID=4, Status=Active.

| № | ИНН | Краткое наименование | Город | Сайт |
|---|-----|---------------------|-------|------|
| 1 | 7726030449 | АО «НРК - Р.О.С.Т.» | Москва | [rrost.ru](https://www.rrost.ru) |
| 2 | 5610083568 | АО ВТБ Регистратор | Москва | [vtbreg.com](https://www.vtbreg.com) |
| 3 | 7704028206 | АО «Реестр» | Москва | [aoreestr.ru](https://www.aoreestr.ru) |
| 4 | 7705397301 | ООО «Реестр-РН» | Москва | [reestrrn.ru](https://www.reestrrn.ru) |
| 5 | 7704011964 | АО «ДРАГА» | Санкт-Петербург | [draga.ru](http://draga.ru) |
| 6 | 7703119309 | АО «АЭИ «ПРАЙМ» | Москва | [prime-interfax.ru](https://prime-interfax.ru) |
| 7 | 7707179242 | АО «СТАТУС» | Москва | [rostatus.ru](https://www.rostatus.ru) |
| 8 | 1901003859 | АО «МРЦ» | Москва | [mrz.ru](https://www.mrz.ru) |
| 9 | 7703802628 | ООО «Регистратор "Гарант"» | Москва | [invest.reggarant.ru](https://invest.reggarant.ru) |
| 10 | 7726050935 | АО РК «Центр-Инвест» | Москва | [centr-invest.ru](https://www.centr-invest.ru) |
| 11 | 7719263354 | АО «Новый регистратор» | Москва | [newreg.ru](https://www.newreg.ru) |
| 12 | 3302021034 | АО «Индустрия-РЕЕСТР» | Москва | [industria-reestr.ru](https://www.industria-reestr.ru) |
| 13 | 7723103642 | АО «РДЦ ПАРИТЕТ» | Москва | [paritet.ru](https://www.paritet.ru) |
| 14 | 5903027161 | АО «РЕГИСТРАТОР ИНТРАКО» | Пермь | [intraco.ru](https://www.intraco.ru) |
| 15 | 6659035711 | АО «Регистратор-Капитал» | Екатеринбург | [regkap.ru](https://www.regkap.ru) |
| 16 | 6661049239 | АО «ВРК» | Екатеринбург | [vrk.ru](https://www.vrk.ru) |
| 17 | 3821010220 | АО «ПРЦ» | Москва | [profrc.ru](https://www.profrc.ru) |
| 18 | 8605006147 | АО «Сервис-Реестр» | Москва | [servis-reestr.ru](https://www.servis-reestr.ru) |
| 19 | 4217027573 | АО «СРК «КОМПАС» | Кемерово | [in-ko.ru](https://www.in-ko.ru) |
| 20 | 5407175878 | АО «РТ-Регистратор» | Москва | [rtreg.ru](https://www.rtreg.ru) |
| 21 | 1435001668 | АО РСР «ЯФЦ» | Якутск | [yfc.ru](https://www.yfc.ru) |
| 22 | 7107039003 | АО «Агентство «РНР»» | Липецк | [a-rnr.ru](https://www.a-rnr.ru) |
| 23 | 9714072529 | АО «Вторая линия» | Москва | [line2.ru](https://line2.ru) |
| 24 | 9718273177 | АО «ФРК» | Москва | [frcreg.ru](http://www.frcreg.ru) |
| 25 | 9702074105 | АО «Реестр-Протон» | Москва | [reestr-proton.ru](https://www.reestr-proton.ru) |
| 26 | 9703197607 | АО «СДК «Сириус»» | Москва | [sdksirius.ru](https://www.sdksirius.ru) |
| 27 | 8602039063 | АО «Сургутинвестнефть» | Сургут | [sineft.ru](https://www.sineft.ru) |
| 28 | 1660055801 | ООО «ЕАР» | Казань | [earc.ru](https://www.earc.ru) |
| 29 | 7731513346 | ООО «Оборонрегистр» | Москва | [oboronregistr.ru](http://oboronregistr.ru) |
| 30 | 3528218586 | ООО «ПАРТНЁР» | Череповец | [partner-reestr.ru](https://www.partner-reestr.ru) |
| 31 | 7842521215 | ООО «ЦУР» | Санкт-Петербург | [rrcentre.ru](https://www.rrcentre.ru) |
| 32 | 6166032022 | ООО «ЮРР» | Ростов-на-Дону | [ug-rr.ru](https://ug-rr.ru) |
| 33 | 7708822233 | ООО «Московский Фондовый Центр» | Москва | [srmfc.ru](https://www.srmfc.ru) |
| 34 | 9704154155 | ООО «РБРУ СД» | Москва | [rbru-depository.ru](https://rbru-depository.ru) |
| 35 | 7730337754 | ООО «ТЕМИОН» | Москва | [temion.ru](https://temion.ru) |

---

## Как проверить регистратора

1. **Проверка лицензии.** Актуальный реестр лицензированных участников рынка ценных бумаг публикуется на сайте Банка России в разделе «Реестры»: [cbr.ru/registries/](https://cbr.ru/registries/)

2. **Проверка через SOAP-сервис ЦБ РФ.** Метод `GetFullInfoByINN` с ИНН регистратора возвращает полную карточку: статус, лицензии (VidID=4), членство в СРО, адрес, контакты. Подробнее: [article-cbr-finorg-api.md](article-cbr-finorg-api.md).

3. **Проверка через сервис «Проверить участника финансового рынка».** Сервис Банка России: [cbr.ru/registries/](https://cbr.ru/registries/)

---

## Связанные документы

- [Выплата дивидендов в АО через регистратора](../gantt/dividend-distribution-gantt.md) — диаграмма Ганта процесса выплаты
- [Запрос списка акционеров у регистратора](../business-processes/shareholders-list-request.md) — бизнес-процесс
- [Ключевые статьи о регистраторе (39-ФЗ)](article-39fz-registrar.md) — ст. 8, 8.6-1, 8.9
- [Веб-сервис ЦБ РФ (FinOrg)](article-cbr-finorg-api.md) — SOAP-интерфейс для проверки участников рынка

---

← [Назад к пояснениям](README.md)
