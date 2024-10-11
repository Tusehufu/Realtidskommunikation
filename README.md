Kunden har bett om chattmöjlighet med kunder samt att kunna få en uppdaterad väderprognos direkt i appen. För att tillgodose kundens önskemål implementerades två komponenter.

Beskrivning av tillagda komponenter:

Väderkomponenten:
Väderkomponenten hämtas användarens platsdata via webbläsarens geolocation, skickar informationen till servern och får tillbaka väderdata i realtid.
Väderkomponenten är byggd med SignalR och kan därför uppdatera väderinformationen i realtid. 
Väderkomponenten översätter även väderinformationen till svenska. 

Chatkomponenten:
Chatkomponenten är designad i en bootstrap modal och är byggd med SignalR vilket innebär att användarna kan skicka meddelanden till varandra i realtid. 
Chatten är en chatt för alla användare där det inte krävs någon autentisering utan endast ett val av alias i chatten.
Chatten sparas i sessionstorage för att användarna ska kunna minimera chattmodalen utan att data försvinner. 

Starta projektet:
Använd kommandot npm install i kommandotolken i rotmappen.
Använd kommandot npm install bootstrap.
Starta backenden i visual studio.
Kör npm run serve i kommandotolken i rotmappen för att köra backenden.
