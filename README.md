# Runekeep API

Backend di [Runekeep](https://github.com/AlessioBucheri/runekeep-app), in **C# / .NET 10**.

Ogni giorno pubblica due file che l'app legge all'avvio (con cache offline):

- `prices.json`: il price guide pubblico di Cardmarket per Riftbound;
- `meta-decks.json`: le decklist dei tornei pubblicate da Riot negli articoli «Top Decks» di playriftbound.com.

**URL pubblici:** https://alessiobucheri.github.io/runekeep-api/prices.json ·
https://alessiobucheri.github.io/runekeep-api/meta-decks.json

Lo stesso sito pubblica anche la pagina di presentazione dell'app e la privacy policy (cartella `website/`):

- https://alessiobucheri.github.io/runekeep-api/ — presentazione con le schermate (URL del prodotto per Riot)
- https://alessiobucheri.github.io/runekeep-api/privacy.html — privacy policy (richiesta dal Play Store)

## Come funziona

```
Cardmarket (file pubblici, game id 22)          GitHub Actions, ogni giorno alle 03:41 UTC
  price_guide_22.json  ─┐
                        ├─► Runekeep.PriceJob ─► site/prices.json ─► GitHub Pages ─► app
  products_singles_22.json ┘
```

- Nessun server da tenere acceso e nessun costo: GitHub Actions è gratuito per i repository pubblici.
- Se Cardmarket non risponde, il job fallisce senza pubblicare: resta online il file del giorno prima.

## Progetti

| Progetto                   | Ruolo                                                                    |
| -------------------------- | ------------------------------------------------------------------------ |
| `src/Runekeep.Prices`      | Libreria: client dei file Cardmarket e trasformazione in snapshot (pura) |
| `src/Runekeep.PriceJob`    | Console: scarica, trasforma, scrive `prices.json`                        |
| `tests/Runekeep.Prices.Tests` | Test xUnit con copie ridotte dei file reali                           |
| `src/Runekeep.Meta`        | Libreria: trova gli articoli «Top Decks» nella sitemap Riot e ne legge le decklist |
| `src/Runekeep.MetaJob`     | Console: scarica gli articoli e scrive `meta-decks.json`                 |
| `tests/Runekeep.Meta.Tests` | Test xUnit sui formati degli articoli (vecchio e attuale)               |

La logica sta tutta nella libreria: quando servirà un'API ASP.NET vera (account, sincronizzazione), la userà
così com'è.

## Formato di `prices.json`

```json
{
  "version": 1,
  "source": "cardmarket",
  "priceDate": "2026-10-09T02:45:56+02:00",
  "generatedAt": "2026-10-09T03:41:12+00:00",
  "count": 1234,
  "prices": { "845712": { "avg": 0.04, "trend": 0.04, "low": 0.02, "foil": 0.17 } }
}
```

Le chiavi di `prices` sono gli `idProduct` di Cardmarket, che sono anche gli id carta nell'app.
`avg` è la media degli ultimi 30 giorni (o la media generale se manca), `foil` la media 30 giorni del foil;
`0` significa nessun dato.

## Formato di `meta-decks.json`

```json
{
  "version": 1,
  "source": "playriftbound.com",
  "generatedAt": "2026-10-12T03:41:40+00:00",
  "events": [
    {
      "id": "los-angeles-top-decks",
      "name": "Los Angeles",
      "title": "Los Angeles’ Top Decks",
      "date": "2026-09-30T16:00:00+00:00",
      "url": "https://playriftbound.com/en-us/news/organizedplay/los-angeles-top-decks/",
      "decks": [
        {
          "player": "DSG Prismaticism", "section": "Top 8 Decks",
          "overallRank": 1, "legendRank": 1, "legendPlayers": 92,
          "legend": "Rengar, Pridestalker", "champion": "Rengar, Trophy Hunter",
          "main": [{ "n": 3, "name": "Inferna" }], "battlefields": [], "runes": [], "sideboard": []
        }
      ]
    }
  ]
}
```

Le carte restano col nome stampato: è l'app che le collega al suo catalogo. Negli articoli più vecchi manca la
leggenda (`legend` assente) e l'app la ricava dal campione. Il job scarica la sitemap e pochi articoli al giorno,
uno alla volta; se il sito Riot non risponde i prezzi vengono pubblicati lo stesso.

## Comandi

```bash
dotnet build
dotnet test
dotnet run --project src/Runekeep.PriceJob -- site   # scrive site/prices.json
dotnet run --project src/Runekeep.MetaJob -- site    # scrive site/meta-decks.json
```

Il job si può lanciare a mano da GitHub: _Actions → Prezzi giornalieri → Run workflow_.

## Da sapere

GitHub sospende i workflow pianificati dei repository pubblici dopo 60 giorni senza attività nel repo e
manda un'email: basta riattivarlo da _Actions_ (o fare un commit).
