KebabBackend � REST API dla restauracji z zam�wieniami online


G��wne funkcjonalno�ci:
	**Zarz�dzanie menu i sk�adnikami** � mi�sa, sosy, dodatki, rozmiary, kategorie.
	**Autoryzacja JWT + Refresh Token** � bezpieczne logowanie i utrzymanie sesji.
	**P�atno�ci Stripe (Checkout + Webhook)** � u�ytkownicy mog� op�aca� zam�wienia online.
	**Obliczanie odleg�o�ci (Azure Maps)** � dynamiczne przeliczanie dostawy.
	**Przechowywanie obraz�w (Azure Blob Storage)** � zdj�cia kebab�w i profili.
	**Email Service z szablonami HTML** � potwierdzenia zam�wie� i powiadomienia.
	**Dashboard z SignalR** � aktualizacje zam�wie� w czasie rzeczywistym dla personelu.
	**Logowanie zdarze� z Serilog** � logi w plikach dziennika.

Stos technologiczny:
	Backend ASP.NET Core (.NET 8)       
	ORM                      | Entity Framework Core          
	P�atno�ci                | Stripe API                     
	Odleg�o�ci i mapy        | Azure Maps                     
	Real-time updates        | Azure SignalR + SignalR        
	Emaile                   | MailKit + szablony HTML        
	Przechowywanie obraz�w   | Azure Blob Storage             
	Logowanie                | Serilog              