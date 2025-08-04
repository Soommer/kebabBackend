KebabBackend – REST API dla restauracji z zamówieniami online


G³ówne funkcjonalnoœci:
	**Zarz¹dzanie menu i sk³adnikami** – miêsa, sosy, dodatki, rozmiary, kategorie.
	**Autoryzacja JWT + Refresh Token** – bezpieczne logowanie i utrzymanie sesji.
	**P³atnoœci Stripe (Checkout + Webhook)** – u¿ytkownicy mog¹ op³acaæ zamówienia online.
	**Obliczanie odleg³oœci (Azure Maps)** – dynamiczne przeliczanie dostawy.
	**Przechowywanie obrazów (Azure Blob Storage)** – zdjêcia kebabów i profili.
	**Email Service z szablonami HTML** – potwierdzenia zamówieñ i powiadomienia.
	**Dashboard z SignalR** – aktualizacje zamówieñ w czasie rzeczywistym dla personelu.
	**Logowanie zdarzeñ z Serilog** – logi w plikach dziennika.

Stos technologiczny:
	Backend ASP.NET Core (.NET 8)       
	ORM                      | Entity Framework Core          
	P³atnoœci                | Stripe API                     
	Odleg³oœci i mapy        | Azure Maps                     
	Real-time updates        | Azure SignalR + SignalR        
	Emaile                   | MailKit + szablony HTML        
	Przechowywanie obrazów   | Azure Blob Storage             
	Logowanie                | Serilog              