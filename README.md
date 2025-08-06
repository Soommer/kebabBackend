# Kebab backend - backend do restauracjii kebab
Projekt backendu do aplikacji restauracyjnej kebab, kt�ra umo�liwia zamawianie jedzenia online, u�ytkownik ma mo�liwo�� zam�wienia kebaba, wyboru sosu, mi�sa,
dodatk�w, wyboru dostawy, zap�aty. Admin ma mo�liwo�� zarz�dzania menu. Pracownicy maj� dostaj� zam�wienia w czasie rzeczywistym za pomoc� SignalR.

## Funkcjonalno�ci
- Rejestracja i logowanie admina
- Zarz�dzanie menu (CRUD)
- Zarz�dzanie zam�wieniami (CRUD)
- Zarz�dzanie kategoriami (CRUD)
- Zarz�dzanie u�ytkownikami (CRUD)
- Swagger API dla dokumentacji API
- Obs�uga p�atno�ci (Stripe)
- Obs�uga dostaw (dostawa lub odbi�r osobisty)
- Obs�uga sesji u�ytkownika (token i refreshtoken)
- Wysy�anie e-maili (potwierdzenie zam�wienia i p�atno�ci)
- integracja z baz� danych Azure SQL
- Azure Blob Storage do przechowywania obraz�w
- SignalR do powiadomie� w czasie rzeczywistym
- Azure Maps do sprawdzania odleg�o�ci u�ytkownika od restauracji

## Technologie

- **J�zyk programowania:**  
  - C# 12, .NET 8.0 (ASP.NET Core WebAPI)
- **API:**  
  - REST, JSON, obs�uga plik�w
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
- **P�atno�ci:**  
  - Stripe
- **Powiadomienia w czasie rzeczywistym:**  
  - SignalR
- **Mapy:**  
  - Azure Maps

## Przyk�ady u�ycia API 
	-Swagger API dost�pne pod adresem: ../swagger/index.html
	 