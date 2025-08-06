# Kebab backend - backend do restauracjii kebab
Projekt backendu do aplikacji restauracyjnej kebab, która umo¿liwia zamawianie jedzenia online, u¿ytkownik ma mo¿liwoœæ zamówienia kebaba, wyboru sosu, miêsa,
dodatków, wyboru dostawy, zap³aty. Admin ma mo¿liwoœæ zarz¹dzania menu. Pracownicy maj¹ dostaj¹ zamówienia w czasie rzeczywistym za pomoc¹ SignalR.

## Funkcjonalnoœci
- Rejestracja i logowanie admina
- Zarz¹dzanie menu (CRUD)
- Zarz¹dzanie zamówieniami (CRUD)
- Zarz¹dzanie kategoriami (CRUD)
- Zarz¹dzanie u¿ytkownikami (CRUD)
- Swagger API dla dokumentacji API
- Obs³uga p³atnoœci (Stripe)
- Obs³uga dostaw (dostawa lub odbiór osobisty)
- Obs³uga sesji u¿ytkownika (token i refreshtoken)
- Wysy³anie e-maili (potwierdzenie zamówienia i p³atnoœci)
- integracja z baz¹ danych Azure SQL
- Azure Blob Storage do przechowywania obrazów
- SignalR do powiadomieñ w czasie rzeczywistym
- Azure Maps do sprawdzania odleg³oœci u¿ytkownika od restauracji

## Technologie

- **Jêzyk programowania:**  
  - C# 12, .NET 8.0 (ASP.NET Core WebAPI)
- **API:**  
  - REST, JSON, obs³uga plików
- **Dependency Injection:**  
  - .NET Core DI
- **Dokumentacja:**  
  - Swagger / OpenAPI (Swashbuckle)
- **Logowanie:**  
  - Serilog 
- **Konteneryzacja:**  
  - Docker
- **Kontrola wersji:**  
  - Git
- **Baza danych:**  
  - Azure SQL Database
- **Storage**:  
  - Azure Blob Storage
- **P³atnoœci:**  
  - Stripe
- **Powiadomienia w czasie rzeczywistym:**  
  - SignalR
- **Mapy:**  
  - Azure Maps

## Przyk³ady u¿ycia API 
	-Swagger API dostêpne pod adresem: ../swagger/index.html
	 