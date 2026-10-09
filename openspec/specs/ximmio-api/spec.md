# ximmio-api Specification

## Purpose
Defines the outbound HTTP adapter `TwenteMilieuApi` (port `IAfvalApi`) that talks to `https://wasteapi.ximmio.com/api/`. For the certificate handling of this client see security-transport.

## Requirements

### Requirement: Address id lookup
`HaalUniekAdresIdOpAsync` SHALL POST JSON `{companyCode, postCode, houseNumber}` to `FetchAdress` and return `UniqueId` of the first item of `dataList`. A browser-like User-Agent header MUST be sent.

#### Scenario: Address found
- **WHEN** the response contains a dataList with a UniqueId
- **THEN** that UniqueId of the first entry is returned

#### Scenario: Address not served
- **WHEN** dataList is empty or the UniqueId is empty
- **THEN** an Exception is thrown stating that no unique address id was found and the user should check that the selected AfvalVerwerker serves the address

#### Scenario: HTTP failure
- **WHEN** the API answers with a non-success status
- **THEN** an HttpRequestException is thrown

### Requirement: Calendar fetch
`HaalKalenderOpAsync` SHALL POST `{companyCode, uniqueAddressID, startDate: "<jaar>-01-01", endDate: "<jaar>-12-31"}` to `GetCalendar` and create one AfvalOphaalMoment per date in `pickupDates` of each `dataList` item, using the given postcode and huisnummer.

#### Scenario: Multiple dates
- **WHEN** an item has three pickupDates
- **THEN** three AfvalOphaalMomenten are returned

#### Scenario: Incomplete items
- **WHEN** an item has no `_pickupTypeText` or no `pickupDates`, or a date cannot be parsed
- **THEN** that item or date is skipped without error

### Requirement: Type mapping
The `_pickupTypeText` value SHALL be trimmed and mapped case-insensitively: GREY to GRIJS, GREEN to GROEN, PAPER to PAPIER, PACKAGES to VERPAKKINGEN, TREE to KERSTBOOM, anything else to ONBEKEND.

#### Scenario: Unknown text
- **WHEN** the type text is "SOMETHING"
- **THEN** the type is ONBEKEND

### Requirement: Generated descriptions
Omschrijving SHALL be derived from the type, not from the API: GRIJS "Restafval wordt opgehaald", GROEN "GFT afval wordt opgehaald", PAPIER "Oud papier wordt opgehaald", VERPAKKINGEN "Plastic en drinkpakken worden opgehaald", KERSTBOOM "Kerstboom wordt opgehaald", otherwise "Afval wordt opgehaald".

#### Scenario: Grey type
- **WHEN** a GREY item is mapped
- **THEN** Omschrijving is "Restafval wordt opgehaald"

### Requirement: ForceerVernieuwen ignored by the HTTP adapter
The adapter SHALL accept the `forceerVernieuwen` parameter but always call the API; the bypass is implemented by the cache decorator (see api-cache).

#### Scenario: Direct call
- **WHEN** the adapter is called with either value of forceerVernieuwen
- **THEN** an HTTP request is made
